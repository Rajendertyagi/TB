using Microsoft.UI;
using Microsoft.UI.Input;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.Web.WebView2.Core;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using TB.Features.Tabs;
using TB.Infrastructure;
using WinRT.Interop;
using Windows.Graphics;
using Windows.System;
using Windows.UI.Core;

namespace TB
{
    public sealed partial class MainWindow : Window
    {
        private TabManager? _tabManager;
        private ThemeService _themeService;

        // --- WIN32 OS KEYBOARD HOOK (Bypasses WebView2 Focus Trap) ---
        private delegate IntPtr HookProc(int nCode, IntPtr wParam, IntPtr lParam);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, HookProc lpfn, IntPtr hMod, uint dwThreadId);
        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);
        [DllImport("user32.dll")]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);
        [DllImport("kernel32.dll")]
        private static extern uint GetCurrentThreadId();

        private IntPtr _hookId = IntPtr.Zero;
        private HookProc _hookProc;

        public MainWindow()
        {
            this.InitializeComponent();
            this.RootGrid.Loaded += RootGrid_Loaded;
            this.Closed += MainWindow_Closed;
            _themeService = new ThemeService(AppDomain.CurrentDomain.BaseDirectory);

            IntPtr hwnd = WindowNative.GetWindowHandle(this);
            WindowId windowId = Win32Interop.GetWindowIdFromWindow(hwnd);
            AppWindow appWindow = AppWindow.GetFromWindowId(windowId);

            DisplayArea displayArea = DisplayArea.GetFromWindowId(windowId, DisplayAreaFallback.Nearest);
            RectInt32 workArea = displayArea.WorkArea;
            int w = (int)(workArea.Width * 0.35), h = (int)(workArea.Height * 0.85);
            appWindow.Resize(new SizeInt32(w, h));
            appWindow.Move(new PointInt32((workArea.Width - w) / 2 + workArea.X, (workArea.Height - h) / 2 + workArea.Y));

            this.ExtendsContentIntoTitleBar = true;
            this.SetTitleBar(AppTitleBar);
            _themeService.ApplyNativeTheme(appWindow);
        }

        private void MainWindow_Closed(object sender, WindowEventArgs args)
        {
            if (_hookId != IntPtr.Zero) UnhookWindowsHookEx(_hookId);
        }

        private async void RootGrid_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                string userDataFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AppData");
                Directory.CreateDirectory(userDataFolder);
                var env = await CoreWebView2Environment.CreateWithOptionsAsync(null, userDataFolder, null);
                await ShellWebView.EnsureCoreWebView2Async(env);
                ShellWebView.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;

                _tabManager = new TabManager(ContentGrid, ShellWebView.CoreWebView2);
                ShellWebView.Source = new Uri(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot", "index.html"));

                _hookProc = KeyboardHookCallback;
                _hookId = SetWindowsHookEx(2, _hookProc, IntPtr.Zero, GetCurrentThreadId());
            }
            catch (Exception ex) { Logger.Error($"Startup Error: {ex.Message}"); }
        }

        private IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                bool isKeyDown = ((long)lParam & 0x80000000) == 0;
                if (isKeyDown)
                {
                    VirtualKey key = (VirtualKey)wParam.ToInt32();
                    bool ctrl = (InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Control) & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;
                    bool shift = (InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Shift) & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;
                    bool alt = (InputKeyboardSource.GetKeyStateForCurrentThread(VirtualKey.Menu) & CoreVirtualKeyStates.Down) == CoreVirtualKeyStates.Down;

                    if (ProcessShortcut(key, ctrl, shift, alt)) return (IntPtr)1;
                }
            }
            return CallNextHookEx(_hookId, nCode, wParam, lParam);
        }

        private bool ProcessShortcut(VirtualKey k, bool ctrl, bool shift, bool alt)
        {
            bool handled = false;

            // Tab Management
            if (ctrl && k == VirtualKey.T) { _ = _tabManager!.CreateTabAsync(); handled = true; }
            else if (ctrl && k == VirtualKey.W) { _tabManager?.CloseActiveTab(); handled = true; }
            else if (ctrl && k == VirtualKey.Tab && !shift) { _tabManager?.NextTab(); handled = true; }
            else if (ctrl && shift && k == VirtualKey.Tab) { _tabManager?.PrevTab(); handled = true; }
            else if (ctrl && k >= VirtualKey.Number1 && k <= VirtualKey.Number9) { _tabManager?.SwitchToIndex((int)k - (int)VirtualKey.Number0); handled = true; }

            // Address Bar (Steals native OS focus back from the Tab WebView)
            else if ((ctrl && k == VirtualKey.L) || k == VirtualKey.F6 || (alt && k == VirtualKey.D) || (ctrl && k == VirtualKey.K) || (ctrl && k == VirtualKey.E))
            {
                ShellWebView.Focus(FocusState.Programmatic);
                ShellWebView.CoreWebView2.PostWebMessageAsJson("{ \"action\": \"FOCUS_URL\" }");
                handled = true;
            }
            // Escape (Routes to JS for 2-step Chrome-style logic)
            else if (k == VirtualKey.Escape)
            {
                ShellWebView.CoreWebView2.PostWebMessageAsJson("{ \"action\": \"ESCAPE_PRESSED\" }");
                handled = true;
            }
            else if (ctrl && k == VirtualKey.N) { new MainWindow().Activate(); handled = true; }

            // Navigation
            else if (k == VirtualKey.F5 || (ctrl && k == VirtualKey.R))
            {
                if (shift) _tabManager?.HardReload(); else _tabManager?.Reload();
                handled = true;
            }
            else if (alt && k == VirtualKey.Left) { _tabManager?.Back(); handled = true; }
            else if (alt && k == VirtualKey.Right) { _tabManager?.Forward(); handled = true; }

            // Utilities
            else if (ctrl && k == VirtualKey.P) { _tabManager?.Print(); handled = true; }
            else if (ctrl && k == VirtualKey.U) { _tabManager?.ViewSource(); handled = true; }

            // Zoom
            else if (ctrl && (k == VirtualKey.Add || (int)k == 187)) { _tabManager?.ZoomIn(); handled = true; }
            else if (ctrl && (k == VirtualKey.Subtract || (int)k == 189)) { _tabManager?.ZoomOut(); handled = true; }
            else if (ctrl && k == VirtualKey.Number0) { _tabManager?.ResetZoom(); handled = true; }

            return handled;
        }

        private async void CoreWebView2_WebMessageReceived(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            try
            {
                using var doc = JsonDocument.Parse(e.WebMessageAsJson);
                string action = doc.RootElement.GetProperty("action").GetString() ?? "";

                switch (action)
                {
                    case "SHELL_READY":
                        ShellWebView.CoreWebView2.PostWebMessageAsJson($"{{ \"action\": \"APPLY_THEME\", \"theme\": {_themeService.ThemeJson} }}");
                        if (_tabManager != null) await _tabManager.CreateTabAsync();
                        break;
                    case "NEW_TAB": if (_tabManager != null) await _tabManager.CreateTabAsync(); break;
                    case "SWITCH_TAB": _tabManager?.SwitchTab(doc.RootElement.GetProperty("id").GetInt32()); break;
                    case "CLOSE_TAB": _tabManager?.CloseTab(doc.RootElement.GetProperty("id").GetInt32()); break;
                    case "NAVIGATE": _tabManager?.NavigateActiveTab(doc.RootElement.GetProperty("url").GetString() ?? ""); break;
                    case "CONTEXT_ACTION": _tabManager?.HandleContextAction(doc.RootElement.GetProperty("type").GetString() ?? "", doc.RootElement.GetProperty("id").GetInt32()); break;

                    // JS Requests C# to stop page load
                    case "STOP": _tabManager?.Stop(); break;
                    // JS Requests C# to return native OS focus to the active web page
                    case "FOCUS_TAB": _tabManager?.FocusActiveTab(); break;
                }
            }
            catch (Exception ex) { Logger.Error($"IPC Error: {ex.Message}"); }
        }
    }
}