using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace TradingBrowser
{
    public sealed partial class MainWindow : Window
    {
        private readonly TradingBrowser.BrowserManager _browserManager;

        public MainWindow()
        {
            this.InitializeComponent();

            // Set up borderless dragging hooks tied directly to Row 0
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);

            // Explicit namespace resolution ensures the compiler finds the initialization sequence cleanly
            TradingBrowser.PersistenceEngine.Initialize();
            
            _browserManager = new TradingBrowser.BrowserManager(
                MainBrowserTabs, 
                TabContentDisplayGrid, 
                Omnibox, 
                this.DispatcherQueue
            );

            // Hook up UI Action event bindings
            MainBrowserTabs.AddTabButtonClick += (s, e) => _browserManager.CreateNewTabContext("New Tab", "https://www.tradingview.com/chart/");
            MainBrowserTabs.SelectionChanged += (s, e) => _browserManager.HandleTabSelectionChanged();
            MainBrowserTabs.TabCloseRequested += (s, e) => _browserManager.HandleTabCloseRequested(e, () => this.Close());
            
            Omnibox.QuerySubmitted += (s, e) => TradingBrowser.NavigationController.ProcessOmniboxQuery(s, _browserManager.GetActiveBrowserInstance());
            
            BackButton.Click += (s, e) => _browserManager.GetActiveBrowserInstance()?.GoBack();
            ForwardButton.Click += (s, e) => _browserManager.GetActiveBrowserInstance()?.GoForward();
            RefreshButton.Click += (s, e) => _browserManager.GetActiveBrowserInstance()?.CoreWebView2?.Reload();

            // Drop the pre-compiler placeholder and load your workspace chart layout
            MainBrowserTabs.TabItems.Clear();
            _browserManager.CreateNewTabContext("TradingView", "https://www.tradingview.com/chart/");
        }
    }
}
