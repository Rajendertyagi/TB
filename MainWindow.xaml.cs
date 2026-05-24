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

        // The View-Model acts as the central data state for the UI
        public BrowserViewModel ViewModel { get; } = new BrowserViewModel();

        public MainWindow()
        {
            this.InitializeComponent();
            
            // 1. Initialize UI Frame
            InitializeCustomTitleBar();

            // 2. Initialize Services
            _dbService = new DatabaseService();
            _inputController = new NavigationController(ViewModel);

            // 3. Register Event Handlers
            this.Content.PreviewKeyDown += _inputController.HandleGlobalKeyboardShortcuts;
            this.Content.PointerPressed += (s, e) => _inputController.HandleMouseAuxiliaryInputs(e);
            
            // AUTOMATIC UI UPDATER: Whenever the ActiveTab changes in the ViewModel, refresh the View
            ViewModel.PropertyChanged += (s, e) => { 
                if (e.PropertyName == nameof(BrowserViewModel.ActiveTab)) UpdateActiveBrowserDisplay(); 
            };
            
            // 4. Start Non-Blocking Session Load
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
            // Perform DB I/O on background thread to keep UI interactive
            var savedTabs = await Task.Run(() => _dbService.LoadWorkspaceLayout());

            this.DispatcherQueue.TryEnqueue(() =>
            {
                if (savedTabs != null && savedTabs.Any())
                {
                    foreach (var tab in savedTabs) 
                        ViewModel.OpenTabs.Add(new TabContext(tab.Title, tab.Url));
                    
                    ViewModel.ActiveTab = ViewModel.OpenTabs.FirstOrDefault();
                }
                else
                {
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
                    var webView = new Microsoft.UI.Xaml.Controls.WebView2();
                    
                    // Lazy-initialize the browser engine asynchronously
                    await webView.EnsureCoreWebView2Async();
                    
                    tab.BrowserInstance = webView;
                    webView.NavigationStarting += (s, args) => { tab.CurrentUrl = args.Uri?.ToString() ?? ""; };
                    webView.Source = new Uri(tab.CurrentUrl);
                }
                BrowserGrid.Children.Add(tab.BrowserInstance);
            }
        }
    }
}
