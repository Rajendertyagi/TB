using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Windowing;
using WinRT.Interop;
using System.Linq;

namespace TradingBrowser
{
    public partial class MainWindow : Window
    {
        private AppWindow _appWindow;
        private DatabaseService _dbService;
        
        // Expose the ViewModel for high-performance {x:Bind} data streaming
        public BrowserViewModel ViewModel { get; } = new BrowserViewModel();

        public MainWindow()
        {
            this.InitializeComponent();
            
            // 1. Initialize native DWM frame decorations
            InitializeCustomTitleBar();

            // 2. Initialize high-performance Dapper SQLite service
            _dbService = new DatabaseService();

            // 3. Setup interaction listeners for workspace state management
            RegisterWindowShellEvents();

            // 4. Restore previous session state or launch default
            LoadPreviousSession();
        }

        private void InitializeCustomTitleBar()
        {
            IntPtr hWnd = WindowNative.GetWindowHandle(this);
            Microsoft.UI.WindowId windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
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

        private void RegisterWindowShellEvents()
        {
            // Dynamic UI Re-mounting: When active tab changes in VM, update the UI canvas
            ViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(BrowserViewModel.ActiveTab))
                {
                    UpdateActiveBrowserDisplay();
                }
            };

            // Hook into window closing to auto-save session state via Dapper
            this.Closed += (s, e) =>
            {
                var savedTabs = ViewModel.OpenTabs.Select(t => new SavedTabItem 
                { 
                    Title = t.Title, 
                    Url = t.CurrentUrl, 
                    LastAccessedTicks = DateTime.UtcNow.Ticks 
                });
                _dbService.SaveWorkspaceLayout(savedTabs);
            };
        }

        private void LoadPreviousSession()
        {
            var savedTabs = _dbService.LoadWorkspaceLayout();
            if (savedTabs != null && savedTabs.Any())
            {
                foreach (var tab in savedTabs)
                {
                    ViewModel.OpenTabs.Add(new TabContext(tab.Title, tab.Url));
                }
                ViewModel.ActiveTab = ViewModel.OpenTabs.FirstOrDefault();
            }
            else
            {
                ViewModel.OpenNewTab();
            }
        }

        private void UpdateActiveBrowserDisplay()
        {
            BrowserGrid.Children.Clear();
            if (ViewModel.ActiveTab != null)
            {
                if (ViewModel.ActiveTab.BrowserInstance == null)
                {
                    var webView = new Microsoft.UI.Xaml.Controls.WebView2();
                    ViewModel.ActiveTab.BrowserInstance = webView;
                    
                    // Route URL updates back to MVVM model
                    webView.NavigationStarting += (s, args) => { ViewModel.ActiveTab.CurrentUrl = args.Uri; };
                    webView.Source = new Uri(ViewModel.ActiveTab.CurrentUrl);
                }
                BrowserGrid.Children.Add(ViewModel.ActiveTab.BrowserInstance);
            }
        }
    }
}
