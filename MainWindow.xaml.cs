using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Windowing;
using WinRT.Interop;

namespace TradingBrowser
{
    public partial class MainWindow : Window
    {
        private AppWindow _appWindow;
        
        // Expose the viewmodel property cleanly for the x:Bind compilation target
        public BrowserViewModel ViewModel { get; } = new BrowserViewModel();

        public MainWindow()
        {
            this.InitializeComponent();
            InitializeCustomTitleBar();

            // Track selection changes to dynamically remount the active chromium view frame
            ViewModel.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(BrowserViewModel.ActiveTab))
                {
                    UpdateActiveBrowserDisplay();
                }
            };

            // Spawn first workspace natively inside the MVVM structural scope
            ViewModel.OpenNewTab();
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

        private void UpdateActiveBrowserDisplay()
        {
            BrowserGrid.Children.Clear();
            if (ViewModel.ActiveTab != null)
            {
                // If this tab context doesn't own a concrete hardware control yet, spin one up
                if (ViewModel.ActiveTab.BrowserInstance == null)
                {
                    var webView = new Microsoft.UI.Xaml.Controls.WebView2();
                    ViewModel.ActiveTab.BrowserInstance = webView;
                    
                    // Route address state streams safely back to data model
                    webView.NavigationStarting += (s, args) => { ViewModel.ActiveTab.CurrentUrl = args.Uri; };
                    webView.Source = new Uri(ViewModel.ActiveTab.CurrentUrl);
                }

                BrowserGrid.Children.Add(ViewModel.ActiveTab.BrowserInstance);
            }
        }
    }
}
