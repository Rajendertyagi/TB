using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System;
using System.IO;
using System.Threading.Tasks;
using TB.Core.Browser;
using TB.Helpers;
using TB.Infrastructure;
using TB.Input;
using TB.Services.Interfaces;
using TB.ViewModels;
using Windows.Graphics;
using WinRT.Interop;

namespace TB;

public sealed partial class MainWindow : Window
{
    private bool _initialized;
    private readonly ITabManager _tabManager;
    private readonly IThemeService _themeService;
    private readonly KeyboardShortcutHandler _keyboardHandler;
    private readonly IFlagService _flagService;
    private readonly AppWindow _appWindow;

    private bool _isMouseInChrome;
    private bool _deferredRecalcPending;

    public MainWindow(ITabManager tabManager, IThemeService themeService, KeyboardShortcutHandler keyboardHandler, MainViewModel mainViewModel, IFlagService flagService)
    {
        InitializeComponent();

        _tabManager = tabManager ?? throw new ArgumentNullException(nameof(tabManager));
        _themeService = themeService ?? throw new ArgumentNullException(nameof(themeService));
        _keyboardHandler = keyboardHandler ?? throw new ArgumentNullException(nameof(keyboardHandler));
        _flagService = flagService ?? throw new ArgumentNullException(nameof(flagService));

        RootGrid.DataContext = mainViewModel.Chrome;

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

        // Wire Events
        FindBar.Initialize(_tabManager);

        var chrome = (ChromeViewModel)RootGrid.DataContext;
        chrome.Tabs.CollectionChanged += (_, _) => PublishChromeMetrics();
        TabStrip.SizeChanged += (_, _) => PublishChromeMetrics();

        _keyboardHandler.Registry.FocusAddressBarRequested += OnFocusAddressBarRequested;
        _keyboardHandler.Registry.CloseWindowRequested += OnCloseWindowRequested;
        _keyboardHandler.Registry.ToggleFullScreenRequested += ToggleFullScreen;
        _keyboardHandler.Registry.ToggleCommandPaletteRequested += ToggleCommandPalette;

        _tabManager.UrlChanged += OnUrlChanged;
        _tabManager.TabSwitched += OnTabSwitched;

        RootGrid.PreviewKeyDown += OnRootPreviewKeyDown;

        _keyboardHandler.StartHook();

        this.Closed += async (_, _) =>
        {
            _keyboardHandler.StopHook();
            RootGrid.PreviewKeyDown -= OnRootPreviewKeyDown;
            _tabManager.UrlChanged -= OnUrlChanged;
            _tabManager.TabSwitched -= OnTabSwitched;
            _keyboardHandler.Registry.FocusAddressBarRequested -= OnFocusAddressBarRequested;
            _keyboardHandler.Registry.CloseWindowRequested -= OnCloseWindowRequested;
            _keyboardHandler.Registry.ToggleFullScreenRequested -= ToggleFullScreen;
            _keyboardHandler.Registry.ToggleCommandPaletteRequested -= ToggleCommandPalette;
            await _tabManager.DisposeAsync();
        };

        RootGrid.Loaded += async (_, _) =>
        {
            PublishChromeMetrics();
            await InitializeAsync(tabManager);
        };
        RootGrid.SizeChanged += (_, _) => PublishChromeMetrics();
    }

    private async Task InitializeAsync(ITabManager tabManager)
    {
        if (_initialized) return;
        _initialized = true;
        try
        {
            var userDataFolder = Paths.WebView2CacheDir;
            var baseArgs = "--disable-features=msSmartScreen,EdgeSmartScreen,msEdgeDefender --disable-background-networking --ignore-certificate-errors";
            var activeSwitches = _flagService.GetActiveChromiumSwitches();
            if (activeSwitches.Count > 0)
            {
                baseArgs += " " + string.Join(" ", activeSwitches);
            }

            var options = new Microsoft.Web.WebView2.Core.CoreWebView2EnvironmentOptions
            {
                AdditionalBrowserArguments = baseArgs
            };
            var env = await Microsoft.Web.WebView2.Core.CoreWebView2Environment.CreateWithOptionsAsync(null, userDataFolder, options);

            // 🛡️ MILESTONE 32: Instantiate Registry & Visibility Strategy
            var appBg = Application.Current.Resources["bgAppBrush"] as Brush ?? new SolidColorBrush(Colors.Transparent);

            // Swap to MaskVisibilityStrategy() later to benchmark against Collapsed
            IHostVisibilityStrategy strategy = new CollapsedVisibilityStrategy();

            var registry = new WebViewRegistry(BrowserSurfaceGrid, env, strategy, appBg);

            // Pass the registry instead of the raw Grid and Environment
            await tabManager.InitializeAsync(registry);
        }
        catch (Exception ex)
        {
            Logger.Error($"Initialization error: {ex}");
        }
    }

    private void OnRootPreviewKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (_keyboardHandler.HandleKey(e.Key)) e.Handled = true;
    }

    private void ToggleFullScreen()
    {
        if (_appWindow.Presenter.Kind == AppWindowPresenterKind.FullScreen)
            _appWindow.SetPresenter(AppWindowPresenterKind.Default);
        else
            _appWindow.SetPresenter(AppWindowPresenterKind.FullScreen);
    }

    private void ChromeContainer_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        _isMouseInChrome = true;
        TabStrip.IsMouseInChrome = true;
    }

    private void ChromeContainer_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        var pos = e.GetCurrentPoint(ChromeContainer).Position;
        if (pos.X < 0 || pos.X > ChromeContainer.ActualWidth || pos.Y < 0 || pos.Y > ChromeContainer.ActualHeight)
        {
            _isMouseInChrome = false;
            TabStrip.IsMouseInChrome = false;
            if (_deferredRecalcPending) { _deferredRecalcPending = false; TabStrip.ApplyLayout(); }
        }
    }

    /// <summary>
    /// Thin coordinator: publishes raw OS chrome metrics to the TabStrip
    /// and refreshes drag rectangles. No tab-width math here.
    /// </summary>
    private void PublishChromeMetrics()
    {
        if (_isMouseInChrome) { _deferredRecalcPending = true; return; }
        try
        {
            double scale       = Content?.XamlRoot?.RasterizationScale ?? 1.0;
            int    rightInset  = _appWindow.TitleBar.RightInset;  // raw screen px
            TabStrip.SetChromeMetrics(rightInset, scale);
            UpdateDragRegions(rightInset, scale);
        }
        catch (Exception ex) { Logger.Warning($"PublishChromeMetrics: {ex.Message}"); }
    }

    private void UpdateDragRegions(int rightInsetRaw, double scale)
    {
        try
        {
            var tabStripH  = (int)TabStrip.ActualHeight;
            var width      = (int)RootGrid.ActualWidth;
            var chromeH    = (int)ChromeContainer.ActualHeight;
            var navH       = chromeH - tabStripH;
            if (width <= 0 || navH <= 0) return;

            // ── Rect 1: Full navigation bar row ──────────────────────────────
            var navBarRect = new RectInt32(0, tabStripH, width, navH);

            // ── Rect 2: Drag rail in the tab strip row ────────────────────────
            // Uses WindowChromeLayout for DPI math — no arithmetic in MainWindow.
            int dragRailRaw  = (int)(TB.Controls.LayoutConst.MinimumDragRegionWidth * scale);
            int railStartRaw = width - rightInsetRaw - dragRailRaw;
            int tabStripHRaw = tabStripH > 0 ? (int)(tabStripH * scale) : 0;

            RectInt32[] rects;
            if (railStartRaw > 0 && dragRailRaw > 0 && tabStripHRaw > 0)
                rects = [navBarRect, new RectInt32(railStartRaw, 0, dragRailRaw, tabStripHRaw)];
            else
                rects = [navBarRect];

            _appWindow.TitleBar.SetDragRectangles(rects);
        }
        catch (Exception ex) { Logger.Warning($"UpdateDragRegions: {ex.Message}"); }
    }

    private void OnUrlChanged(object? sender, Models.UrlEventArgs e)
    {
        NavBar.UpdateSecurityIcon(e.Url);
    }

    private void OnTabSwitched(object? sender, Models.TabEventArgs e)
    {
        NavBar.UpdateSecurityIcon(e.Url);
    }

    private void ToggleCommandPalette()
    {
        if (CommandPaletteOverlay.Visibility == Visibility.Visible)
        {
            CommandPaletteOverlay.Visibility = Visibility.Collapsed;
        }
        else
        {
            CommandPaletteOverlay.Visibility = Visibility.Visible;
            CommandPaletteInput.Text = string.Empty;
            
            var allCmds = _keyboardHandler.Registry.Commands;
            CommandPaletteList.ItemsSource = allCmds;
            if (allCmds.Count > 0)
            {
                CommandPaletteList.SelectedIndex = 0;
            }
            
            CommandPaletteInput.Focus(FocusState.Programmatic);
        }
    }

    private void CommandPaletteOverlay_PointerPressed(object sender, PointerRoutedEventArgs e)
    {
        if (ReferenceEquals(e.OriginalSource, CommandPaletteOverlay))
        {
            CommandPaletteOverlay.Visibility = Visibility.Collapsed;
        }
    }

    private void CommandPaletteInput_TextChanged(object sender, TextChangedEventArgs e)
    {
        var filter = CommandPaletteInput.Text;
        var allCmds = _keyboardHandler.Registry.Commands;
        if (string.IsNullOrWhiteSpace(filter))
        {
            CommandPaletteList.ItemsSource = allCmds;
        }
        else
        {
            var filtered = new List<CommandMetadata>();
            for (int i = 0; i < allCmds.Count; i++)
            {
                var cmd = allCmds[i];
                if (cmd.Name.Contains(filter, StringComparison.OrdinalIgnoreCase) || 
                    cmd.Description.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                    cmd.Shortcut.Contains(filter, StringComparison.OrdinalIgnoreCase))
                {
                    filtered.Add(cmd);
                }
            }
            CommandPaletteList.ItemsSource = filtered;
        }

        if (CommandPaletteList.Items.Count > 0)
        {
            CommandPaletteList.SelectedIndex = 0;
        }
    }

    private void CommandPaletteInput_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Escape)
        {
            CommandPaletteOverlay.Visibility = Visibility.Collapsed;
            e.Handled = true;
        }
        else if (e.Key == Windows.System.VirtualKey.Down)
        {
            if (CommandPaletteList.Items.Count > 0)
            {
                CommandPaletteList.Focus(FocusState.Programmatic);
            }
            e.Handled = true;
        }
        else if (e.Key == Windows.System.VirtualKey.Enter)
        {
            ExecuteSelectedCommand();
            e.Handled = true;
        }
    }

    private void CommandPaletteList_KeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key == Windows.System.VirtualKey.Escape)
        {
            CommandPaletteOverlay.Visibility = Visibility.Collapsed;
            e.Handled = true;
        }
        else if (e.Key == Windows.System.VirtualKey.Enter)
        {
            ExecuteSelectedCommand();
            e.Handled = true;
        }
        else if (e.Key == Windows.System.VirtualKey.Up && CommandPaletteList.SelectedIndex == 0)
        {
            CommandPaletteInput.Focus(FocusState.Programmatic);
            e.Handled = true;
        }
    }

    private void CommandPaletteList_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
    {
        ExecuteSelectedCommand();
        e.Handled = true;
    }

    private void ExecuteSelectedCommand()
    {
        if (CommandPaletteList.SelectedItem is CommandMetadata selected)
        {
            CommandPaletteOverlay.Visibility = Visibility.Collapsed;
            _keyboardHandler.Registry.Execute(selected.Command);
        }
    }

    private void OnFocusAddressBarRequested() => NavBar.FocusUrlBar();
    private void OnCloseWindowRequested() => Close();
}

