using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Windowing;
using WinRT.Interop;
using Microsoft.UI.Dispatching;

namespace TradingBrowser
{
    public sealed partial class MainWindow : Window
    {
        private AppWindow _appWindow;
        private readonly DatabaseService _dbService;
        private readonly NavigationController _inputController;

        // Public property required for x:Bind resolution
        public BrowserViewModel ViewModel { get; } = new BrowserViewModel();

        public MainWindow()
        {
            this.InitializeComponent();
            
            InitializeCustomTitleBar();

            _dbService = new DatabaseService();
            _inputController = new NavigationController(ViewModel);

            // Register input routing
            this.Content.PreviewKeyDown += _inputController.HandleGlobalKeyboardShortcuts;
            this.Content.PointerPressed += (s, e) => _inputController.HandleMouseAuxiliaryInputs(e);
            
            // Event listener for tab changes
            ViewModel.PropertyChanged += (s, e) => { 
                if (e.PropertyName == nameof(BrowserViewModel.ActiveTab)) UpdateActiveBrowserDisplay(); 
            };
            
            LoggerService.Info("System: UI components initialized. Loading session data...");
            _ = LoadPreviousSessionAsync();
        }

        private void InitializeCustomTitleBar()
        {
            IntPtr hWnd = WindowNative.GetWindowHandle(this);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
            _appWindow = AppWindow.GetFromWindowId(windowId);

            if (AppWindowTitleBar.IsCustomizationSupported())
            {
                var titleBar = _appWindow.TitleBar;
                titleBar.ExtendsContentIntoTitleBar = true;
                titleBar.ButtonBackgroundColor = Microsoft.UI.Colors.Transparent;
                titleBar.ButtonInactiveBackgroundColor = Microsoft.UI.Colors.Transparent;
                titleBar.ButtonForegroundColor = Microsoft.UI.Colors.White;
            }
        }

        private async Task LoadPreviousSessionAsync()
        {
            var savedTabs = await Task.Run(() => {
                LoggerService.Info("Database: Fetching saved workspace...");
                return _dbService.LoadWorkspaceLayout();
            });

            this.DispatcherQueue.TryEnqueue(() =>
            {
                if (savedTabs != null && savedTabs.Any())
                {
                    LoggerService.Info($"System: Restoring {savedTabs.Count} active workspaces.");
                    foreach (var tab in savedTabs) 
                        ViewModel.OpenTabs.Add(new TabContext(tab.Title, tab.Url));
                    
                    ViewModel.ActiveTab = ViewModel.OpenTabs.FirstOrDefault();
                }
                else
                {
                    LoggerService.Info("System: No saved session found. Opening default workspace.");
                    ViewModel.OpenNewTab();
                }
            });
        }

        private async void UpdateActiveBrowserDisplay()
        {
            BrowserGrid.Children.Clear();
            var tab = ViewModel.ActiveTab;

            if (tab != null)
            {
                if (tab.BrowserInstance == null)
                {
                    LoggerService.Info($"Engine: Warming up Chromium for workspace: {tab.Title}");
                    var webView = new Microsoft.UI.Xaml.Controls.WebView2();
                    
                    await webView.EnsureCoreWebView2Async();
                    
                    tab.BrowserInstance = webView;
                    webView.NavigationStarting += (s, args) => { tab.CurrentUrl = args.Uri?.ToString() ?? ""; };
                    webView.Source = new Uri(tab.CurrentUrl);
                    LoggerService.Info("Engine: Browser control successfully mounted.");
                }
                BrowserGrid.Children.Add(tab.BrowserInstance);
            }
        }
    }
}
