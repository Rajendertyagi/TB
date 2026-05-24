using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Windowing;
using WinRT.Interop;
using System.Linq;

namespace TradingBrowser
{
    public sealed partial class MainWindow : Window
    {
        private AppWindow _appWindow;
        private DatabaseService _dbService;
        private NavigationController _inputController;

        public BrowserViewModel ViewModel { get; } = new BrowserViewModel();

        public MainWindow()
        {
            this.InitializeComponent();
            InitializeCustomTitleBar();

            _dbService = new DatabaseService();
            _inputController = new NavigationController(ViewModel);

            // Hook input events to the isolated controller
            this.Content.PreviewKeyDown += _inputController.HandleGlobalKeyboardShortcuts;
            this.Content.PointerPressed += (s, e) => _inputController.HandleMouseAuxiliaryInputs(e);

            ViewModel.PropertyChanged += (s, e) => { if (e.PropertyName == nameof(BrowserViewModel.ActiveTab)) UpdateActiveBrowserDisplay(); };
            
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

        private void LoadPreviousSession()
        {
            var savedTabs = _dbService.LoadWorkspaceLayout();
            if (savedTabs != null && savedTabs.Any())
            {
                foreach (var tab in savedTabs) ViewModel.OpenTabs.Add(new TabContext(tab.Title, tab.Url));
                ViewModel.ActiveTab = ViewModel.OpenTabs.FirstOrDefault();
            }
            else ViewModel.OpenNewTab();
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
                    webView.NavigationStarting += (s, args) => { ViewModel.ActiveTab.CurrentUrl = args.Uri?.ToString() ?? ""; };
                    webView.Source = new Uri(ViewModel.ActiveTab.CurrentUrl);
                }
                BrowserGrid.Children.Add(ViewModel.ActiveTab.BrowserInstance);
            }
        }
    }
}
