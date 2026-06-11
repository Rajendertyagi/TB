using Microsoft.UI; // FIX: Required for Win32Interop
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using System;
using System.IO;
using System.Threading.Tasks;
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
    private readonly AppWindow _appWindow;

    private bool _isMouseInChrome;
    private bool _deferredRecalcPending;

    public MainWindow(ITabManager tabManager, IThemeService themeService, KeyboardShortcutHandler keyboardHandler, MainViewModel mainViewModel)
    {
        InitializeComponent();

        _tabManager = tabManager ?? throw new ArgumentNullException(nameof(tabManager));
        _themeService = themeService ?? throw new ArgumentNullException(nameof(themeService));
        _keyboardHandler = keyboardHandler ?? throw new ArgumentNullException(nameof(keyboardHandler));

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
        var chrome = (ChromeViewModel)RootGrid.DataContext;
        chrome.Tabs.CollectionChanged += (_, _) => QueueRecalc();
        TabStrip.SizeChanged += (_, _) => QueueRecalc();

        _keyboardHandler.FocusUrlBarRequested += () => NavBar.FocusUrlBar();
        _keyboardHandler.CloseWindowRequested += () => Close();
        _keyboardHandler.ToggleFullScreenRequested += ToggleFullScreen;

        _tabManager.UrlChanged += (_, e) => NavBar.UpdateSecurityIcon(e.Url);
        _tabManager.TabSwitched += (_, e) => NavBar.UpdateSecurityIcon(e.Url);

        RootGrid.PreviewKeyDown += OnRootPreviewKeyDown;

        this.Closed += async (_, _) =>
        {
            RootGrid.PreviewKeyDown -= OnRootPreviewKeyDown;
            _tabManager.UrlChanged -= (_, e) => NavBar.UpdateSecurityIcon(e.Url);
            _tabManager.TabSwitched -= (_, e) => NavBar.UpdateSecurityIcon(e.Url);
            await _tabManager.DisposeAsync();
        };

        RootGrid.Loaded += async (_, _) =>
        {
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
            var userDataFolder = Path.Combine(basePath, "AppData", "WebView2Cache");
            Directory.CreateDirectory(userDataFolder);
            var env = await Microsoft.Web.WebView2.Core.CoreWebView2Environment.CreateWithOptionsAsync(null, userDataFolder, null);
            await tabManager.InitializeAsync(ContentGrid, env);
        }
        catch (Exception ex) { Logger.Error($"Initialization error: {ex}"); }
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

    private void ChromeContainer_PointerEntered(object sender, PointerRoutedEventArgs e) => _isMouseInChrome = true;

    private void ChromeContainer_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        var pos = e.GetCurrentPoint(ChromeContainer).Position;
        if (pos.X < 0 || pos.X > ChromeContainer.ActualWidth || pos.Y < 0 || pos.Y > ChromeContainer.ActualHeight)
        {
            _isMouseInChrome = false;
            if (_deferredRecalcPending) { _deferredRecalcPending = false; TabStrip.RecalculateTabWidths(); }
        }
    }

    private void QueueRecalc()
    {
        if (_isMouseInChrome) { _deferredRecalcPending = true; return; }
        TabStrip.RecalculateTabWidths();
    }

    private void UpdateDragRegions()
    {
        try
        {
            var tabStripH = (int)TabStrip.ActualHeight;
            var width = (int)RootGrid.ActualWidth;
            var chromeH = (int)ChromeContainer.ActualHeight;
            var navH = chromeH - tabStripH;
            if (width <= 0 || navH <= 0) return;
            _appWindow.TitleBar.SetDragRectangles(new[] { new RectInt32(0, tabStripH, width, navH) });
        }
        catch (Exception ex) { Logger.Warning($"Failed to set drag regions: {ex.Message}"); }
    }
}
