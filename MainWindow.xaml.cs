using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.Web.WebView2.Core;
using TB.Helpers;
using TB.Infrastructure;
using TB.Input;
using TB.Services.Interfaces;
using TB.ViewModels;
using Windows.Foundation;
using Windows.Graphics;
using Windows.System;
using WinRT.Interop;

namespace TB;

public sealed partial class MainWindow : Window
{
    private bool _initialized;
    private readonly ITabManager _tabManager;
    private readonly IThemeService _themeService;
    private readonly KeyboardShortcutHandler _keyboardHandler;
    private readonly MainViewModel _mainViewModel;
    private readonly AppWindow _appWindow;

    private const int GWLP_WNDPROC = -4;
    private const int GWL_STYLE = -16;
    private const long WS_SYSMENU = 0x00080000;
    private const uint SWP_FRAMECHANGED = 0x0020;
    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_NOSIZE = 0x0001;
    private const int WM_KEYDOWN = 0x0100;
    private const int WM_KEYUP = 0x0101;
    private const int WM_SYSKEYDOWN = 0x0104;
    private const int WM_SYSKEYUP = 0x0105;
    private const int WM_NCRBUTTONDOWN = 0x00A4;

    private const int WH_KEYBOARD_LL = 13;

    private IntPtr _oldWndProc;
    private WndProcDelegate _wndProcDelegate;
    private IntPtr _keyboardHook;
    private LowLevelKeyboardProc _keyboardHookDelegate;

    private bool _isMouseInChrome;
    private bool _deferredRecalcPending;

    [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr", CharSet = CharSet.Auto)]
    private static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr", CharSet = CharSet.Auto)]
        private static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr GetModuleHandle(string? lpModuleName);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("user32.dll")]
    private static extern short GetAsyncKeyState(int virtualKey);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate IntPtr WndProcDelegate(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    [UnmanagedFunctionPointer(CallingConvention.Winapi)]
    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential)]
    private struct KBDLLHOOKSTRUCT
    {
        public uint vkCode;
        public uint scanCode;
        public uint flags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    private const int HC_ACTION = 0;
    private readonly uint _processId = (uint)Environment.ProcessId;

    private IntPtr LowLevelKeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode == HC_ACTION)
        {
            uint msg = (uint)wParam;
            if (msg == WM_KEYDOWN || msg == WM_SYSKEYDOWN)
            {
                var hookStruct = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);
                var key = (VirtualKey)hookStruct.vkCode;
                Logger.Debug($"[LLKBHOOK] msg={(msg == WM_KEYDOWN ? "WM_KEYDOWN" : "WM_SYSKEYDOWN")} VirtualKey={key}");

                // Only intercept keys destined for our process
                var foregroundHwnd = GetForegroundWindow();
                if (foregroundHwnd != IntPtr.Zero)
                {
                    GetWindowThreadProcessId(foregroundHwnd, out var targetPid);
                    if (targetPid != _processId)
                    {
                        Logger.Debug($"[LLKBHOOK]   -> not our process (pid={targetPid}), passing through");
                        return CallNextHookEx(_keyboardHook, nCode, wParam, lParam);
                    }
                }

                if (_keyboardHandler.HandleKey(key))
                {
                    Logger.Debug($"[LLKBHOOK]   -> HANDLED, returning 1");
                    return (IntPtr)1;
                }
            }
        }
        return CallNextHookEx(_keyboardHook, nCode, wParam, lParam);
    }

    private IntPtr WndProcHook(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam)
    {
        if (msg == WM_KEYDOWN || msg == WM_SYSKEYDOWN)
        {
            var key = (VirtualKey)(int)wParam;
            string msgName = msg == WM_KEYDOWN ? "WM_KEYDOWN" : "WM_SYSKEYDOWN";
            Logger.Debug($"[WNDPROC] {msgName} VirtualKey={key} ({(int)key})");

            bool isModifier = key == VirtualKey.Control || key == VirtualKey.LeftControl ||
                              key == VirtualKey.RightControl || key == VirtualKey.Menu ||
                              key == VirtualKey.LeftMenu || key == VirtualKey.RightMenu ||
                              key == VirtualKey.Shift || key == VirtualKey.LeftShift ||
                              key == VirtualKey.RightShift;

            if (isModifier)
            {
                Logger.Debug($"[WNDPROC]   -> modifier key, polling live state in HandleKey");
            }
            else
            {
                bool matched = _keyboardHandler.HandleKey(key);
                Logger.Debug($"[WNDPROC]   -> HandleKey({key}) returned {matched}");
                if (matched)
                    return IntPtr.Zero;
            }
        }
        else if (msg == WM_KEYUP || msg == WM_SYSKEYUP)
        {
            var key = (VirtualKey)(int)wParam;
            string msgName = msg == WM_KEYUP ? "WM_KEYUP" : "WM_SYSKEYUP";
            Logger.Debug($"[WNDPROC] {msgName} VirtualKey={key} (live poll only, no state tracking)");
        }
        else if (msg == WM_NCRBUTTONDOWN)
        {
            // Suppress DWM system menu on extended title bar — handled via PointerPressed
            return (IntPtr)1;
        }

        return CallWindowProc(_oldWndProc, hWnd, msg, wParam, lParam);
    }

    public MainWindow(ITabManager tabManager, IThemeService themeService, KeyboardShortcutHandler keyboardHandler, MainViewModel mainViewModel)
    {
        InitializeComponent();

        _tabManager = tabManager ?? throw new ArgumentNullException(nameof(tabManager));
        _themeService = themeService ?? throw new ArgumentNullException(nameof(themeService));
        _keyboardHandler = keyboardHandler ?? throw new ArgumentNullException(nameof(keyboardHandler));
        _mainViewModel = mainViewModel ?? throw new ArgumentNullException(nameof(mainViewModel));

        RootGrid.DataContext = _mainViewModel.Chrome;

        var hwnd = WindowNative.GetWindowHandle(this);
        var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
        _appWindow = AppWindow.GetFromWindowId(windowId);
        var displayArea = DisplayArea.GetFromWindowId(windowId, DisplayAreaFallback.Nearest);

        _appWindow.TitleBar.ExtendsContentIntoTitleBar = true;
        _themeService.ApplyNativeTheme(_appWindow);

        var workArea = displayArea.WorkArea;
        var w = (int)(workArea.Width * 0.35);
        var h = (int)(workArea.Height * 0.85);
        _appWindow.Resize(new SizeInt32(w, h));
        _appWindow.Move(new PointInt32((workArea.Width - w) / 2 + workArea.X, (workArea.Height - h) / 2 + workArea.Y));

        TabStripGrid.Margin = new Thickness(0, 0, _appWindow.TitleBar.RightInset, 0);

        var chrome = (ChromeViewModel)RootGrid.DataContext;
        chrome.Tabs.CollectionChanged += (_, _) => QueueRecalc();
        TabStripGrid.SizeChanged += (_, _) => QueueRecalc();
        TabRepeater.ElementPrepared += (_, args) =>
        {
            if (args.Element is Button btn)
            {
                btn.Width = chrome.TabWidth;
                btn.PointerEntered += (_, _) =>
                {
                    if (btn.DataContext is TabItemViewModel vm)
                        vm.IsHovered = true;
                };
                btn.PointerExited += (_, _) =>
                {
                    if (btn.DataContext is TabItemViewModel vm)
                        vm.IsHovered = false;
                };
                btn.PointerPressed += (_, e) =>
                {
                    try
                    {
                        var pt = e.GetCurrentPoint(btn);
                        if (pt.Properties.IsRightButtonPressed)
                        {
                            e.Handled = true;
                            if (btn.DataContext is TabItemViewModel vm && RootGrid.DataContext is ChromeViewModel chrome)
                                ShowTabContextMenu(btn, vm, chrome, pt.Position);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Warning($"Tab pointer handler: {ex.Message}");
                    }
                };
            }
        };

        _keyboardHandler.FocusUrlBarRequested += FocusUrlBar;
        _keyboardHandler.CloseWindowRequested += () => Close();
        _keyboardHandler.ToggleFullScreenRequested += ToggleFullScreen;

        _wndProcDelegate = WndProcHook;
        _oldWndProc = SetWindowLongPtr(hwnd, GWLP_WNDPROC, Marshal.GetFunctionPointerForDelegate(_wndProcDelegate));
        if (_oldWndProc == IntPtr.Zero)
            Logger.Warning("Failed to install WndProc hook");

        _keyboardHookDelegate = LowLevelKeyboardHookCallback;
        _keyboardHook = SetWindowsHookEx(WH_KEYBOARD_LL, _keyboardHookDelegate, GetModuleHandle(null), 0);
        if (_keyboardHook == IntPtr.Zero)
            Logger.Error("Failed to install keyboard hook");

        Closed += async (_, _) =>
        {
            if (_keyboardHook != IntPtr.Zero)
                UnhookWindowsHookEx(_keyboardHook);
            if (_oldWndProc != IntPtr.Zero)
                SetWindowLongPtr(hwnd, GWLP_WNDPROC, _oldWndProc);
            await tabManager.DisposeAsync();
        };

        RootGrid.Loaded += async (_, _) =>
        {
            // Remove WS_SYSMENU after WinUI finishes title bar extension
            var style = GetWindowLongPtr(hwnd, GWL_STYLE);
            SetWindowLongPtr(hwnd, GWL_STYLE, (IntPtr)((long)style & ~WS_SYSMENU));
            SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0, SWP_FRAMECHANGED | SWP_NOMOVE | SWP_NOSIZE);
            UpdateDragRegions();
            await InitializeAsync(tabManager);
        };
        RootGrid.SizeChanged += (_, _) => UpdateDragRegions();
    }

    private async Task InitializeAsync(ITabManager tabManager)
    {
        if (_initialized) return;
        _initialized = true;

        try
        {
            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            var userDataFolder = Path.Combine(basePath, "AppData");
            Directory.CreateDirectory(userDataFolder);

            var env = await CoreWebView2Environment.CreateWithOptionsAsync(null, userDataFolder, null);

            await tabManager.InitializeAsync(ContentGrid, env);
        }
        catch (Exception ex)
        {
            Logger.Error($"Initialization error: {ex}");
        }
    }

    public void FocusUrlBar()
    {
        UrlInput?.Focus(FocusState.Programmatic);
        UrlInput?.SelectAll();
    }

    private void ToggleFullScreen()
    {
        var hwnd = WindowNative.GetWindowHandle(this);
        var windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
        var appWindow = AppWindow.GetFromWindowId(windowId);
        if (appWindow.Presenter.Kind == AppWindowPresenterKind.FullScreen)
            appWindow.SetPresenter(AppWindowPresenterKind.Default);
        else
            appWindow.SetPresenter(AppWindowPresenterKind.FullScreen);
    }

    private static string CurrentThemeKey =>
        Application.Current.RequestedTheme == ApplicationTheme.Light ? "Light" : "Dark";

    private void OnUrlInputGotFocus(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox tb)
        {
            // Show full URL for editing, not the stripped domain
            var vm = (ChromeViewModel)RootGrid.DataContext;
            tb.Text = vm.UrlText;
            tb.SelectAll();
            if (tb.Parent is Grid grid && grid.Parent is Border border)
            {
                var dict = Application.Current.Resources.ThemeDictionaries[CurrentThemeKey] as ResourceDictionary;
                if (dict?.TryGetValue("accentBrush", out var brush) == true)
                    border.BorderBrush = (Microsoft.UI.Xaml.Media.Brush)brush;
            }
        }
    }

    private void OnUrlInputLostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox tb)
        {
            var binding = tb.GetBindingExpression(TextBox.TextProperty);
            binding?.UpdateSource();
            if (tb.Parent is Grid grid && grid.Parent is Border border)
            {
                var dict = Application.Current.Resources.ThemeDictionaries[CurrentThemeKey] as ResourceDictionary;
                if (dict?.TryGetValue("borderCrispBrush", out var brush) == true)
                    border.BorderBrush = (Microsoft.UI.Xaml.Media.Brush)brush;
            }
        }
    }

    private void OnUrlInputKeyDown(object sender, KeyRoutedEventArgs e)
    {
        // Route through global shortcut engine first — matches suppress the ding
        if (_keyboardHandler.HandleKey(e.Key))
        {
            e.Handled = true;
            return;
        }

        if (e.Key == VirtualKey.Enter)
        {
            bool ctrl = (GetAsyncKeyState(0x11) & 0x8000) != 0;
            if (ctrl && UrlInput is not null)
            {
                var text = UrlInput.Text.Trim();
                if (!string.IsNullOrWhiteSpace(text) && !text.Contains('.') && !text.Contains('/'))
                    UrlInput.Text = $"www.{text}.com";
            }
            var vm = (ChromeViewModel)RootGrid.DataContext;
            vm.UrlText = UrlInput?.Text ?? "";
            vm.NavigateCommand.Execute(null);
            e.Handled = true;
        }
    }

    private void ChromeContainer_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        _isMouseInChrome = true;
    }

    private void ChromeContainer_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        var pos = e.GetCurrentPoint(ChromeContainer).Position;
        if (pos.X < 0 || pos.X > ChromeContainer.ActualWidth ||
            pos.Y < 0 || pos.Y > ChromeContainer.ActualHeight)
        {
            _isMouseInChrome = false;
            if (_deferredRecalcPending)
            {
                _deferredRecalcPending = false;
                RecalculateTabWidths();
            }
        }
    }

    private void QueueRecalc()
    {
        if (_isMouseInChrome)
        {
            _deferredRecalcPending = true;
            return;
        }
        RecalculateTabWidths();
    }

    private void ShowTabContextMenu(FrameworkElement element, TabItemViewModel vm, ChromeViewModel chrome, Point point)
    {
        try
        {
            var presenterStyle = (Style)Application.Current.Resources["HeliumMenuFlyoutPresenterStyle"];
            var itemStyle = (Style)Application.Current.Resources["HeliumMenuFlyoutItemStyle"];
            var flyout = new MenuFlyout { MenuFlyoutPresenterStyle = presenterStyle };
            flyout.Items.Add(new MenuFlyoutItem { Text = "New Tab", Command = vm.NewTabCommand, Style = itemStyle });
            flyout.Items.Add(new MenuFlyoutItem { Text = "Duplicate Tab", Command = vm.DuplicateCommand, Style = itemStyle });
            flyout.Items.Add(new MenuFlyoutItem { Text = "Reload", Command = vm.ReloadTabCommand, Style = itemStyle });
            flyout.Items.Add(new MenuFlyoutSeparator());
            flyout.Items.Add(new MenuFlyoutItem { Text = "Close", Command = vm.CloseCommand, Style = itemStyle });
            flyout.Items.Add(new MenuFlyoutItem { Text = "Close Other Tabs", Command = vm.CloseOtherTabsCommand, Style = itemStyle });
            try { if (App.MainWindow is Window w) foreach (var popup in VisualTreeHelper.GetOpenPopups(w)) popup.IsOpen = false; } catch { }
            flyout.ShowAt(element, point);
        }
        catch (Exception ex)
        {
            Logger.Warning($"Tab context menu failed: {ex.Message}");
        }
    }

    private void RecalculateTabWidths()
    {
        var chrome = (ChromeViewModel)TabStripGrid.DataContext;
        chrome.RecalculateTabWidth(TabStripGrid.ActualWidth);
        double w = chrome.TabWidth;
        int count = TabRepeater.ItemsSourceView?.Count ?? 0;
        for (int i = 0; i < count; i++)
        {
            if (TabRepeater.TryGetElement(i) is Button btn)
                btn.Width = w;
        }
        foreach (var tab in chrome.Tabs)
            tab.IsSquashed = w <= 36;
    }

    private void UpdateDragRegions()
    {
        try
        {
            var tabStripH = (int)TabStripGrid.ActualHeight;
            var width = (int)RootGrid.ActualWidth;
            var chromeH = (int)ChromeContainer.ActualHeight;
            var navH = chromeH - tabStripH;
            if (width <= 0 || navH <= 0) return;
            _appWindow.TitleBar.SetDragRectangles(new[] { new RectInt32(0, tabStripH, width, navH) });
            Logger.Debug($"Drag regions set: tabStripH={tabStripH}, navH={navH}, width={width}");
        }
        catch (Exception ex)
        {
            Logger.Warning($"Failed to set drag regions: {ex.Message}");
        }
    }
}
