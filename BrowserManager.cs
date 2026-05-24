using System;
using System.IO;
using System.Collections.Generic;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;

namespace TradingBrowser
{
    public class BrowserManager
    {
        private readonly TabView _tabViewControl;
        private readonly Grid _displayGrid;
        private readonly AutoSuggestBox _omniboxControl;
        private readonly Microsoft.UI.Dispatching.DispatcherQueue _dispatcherQueue;
        
        private readonly Dictionary<TabViewItem, WebView2> _tabBrowserMapping = new Dictionary<TabViewItem, WebView2>();
        public TabViewItem CurrentActiveTabItem { get; private set; } = null;

        public BrowserManager(TabView tabView, Grid displayGrid, AutoSuggestBox omnibox, Microsoft.UI.Dispatching.DispatcherQueue dispatcher)
        {
            _tabViewControl = tabView;
            _displayGrid = displayGrid;
            _omniboxControl = omnibox;
            _dispatcherQueue = dispatcher;
        }

        public void CreateNewTabContext(string tabTitle, string targetUrl)
        {
            var newTabItem = new TabViewItem
            {
                Header = tabTitle,
                IconSource = new SymbolIconSource { Symbol = Symbol.Globe }
            };

            var webBrowserInstance = new WebView2
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };

            _tabBrowserMapping.Add(newTabItem, webBrowserInstance);
            _tabViewControl.TabItems.Add(newTabItem);
            _tabViewControl.SelectedItem = newTabItem;

            _dispatcherQueue.TryEnqueue(async () =>
            {
                try
                {
                    string rootPath = AppDomain.CurrentDomain.BaseDirectory;
                    string cachePath = Path.Combine(rootPath, "WebViewCache");
                    var options = new CoreWebView2EnvironmentOptions();
                    
                    var env = await CoreWebView2Environment.CreateWithOptionsAsync(
                        browserExecutableFolder: string.Empty, 
                        userDataFolder: cachePath, 
                        options: options
                    );
                    
                    await webBrowserInstance.EnsureCoreWebView2Async(env);
                    webBrowserInstance.CoreWebView2.Settings.IsWebMessageEnabled = true;
                    
                    // OPTION A POPUP ROUTING ENGINE
                    webBrowserInstance.CoreWebView2.NewWindowRequested += (s, e) =>
                    {
                        e.Handled = true; 
                        _dispatcherQueue.TryEnqueue(() =>
                        {
                            CreateNewTabContext("Loading Workspace...", e.Uri);
                        });
                    };

                    webBrowserInstance.CoreWebView2.SourceChanged += (s, e) =>
                    {
                        if (_tabViewControl.SelectedItem == newTabItem)
                        {
                            _omniboxControl.Text = webBrowserInstance.Source.ToString();
                        }
                    };

                    webBrowserInstance.CoreWebView2.NavigationCompleted += (s, e) =>
                    {
                        if (e.IsSuccess)
                        {
                            newTabItem.Header = webBrowserInstance.CoreWebView2.DocumentTitle;
                        }
                    };
                    
                    webBrowserInstance.Source = new Uri(targetUrl);
                }
                catch (Exception ex)
                {
                    Logger.WriteLog("Global Errors", $"WebView instantiation error: {ex.Message}");
                }
            });
        }

        public void HandleTabSelectionChanged()
        {
            if (_tabViewControl.SelectedItem is TabViewItem selectedTab && _tabBrowserMapping.ContainsKey(selectedTab))
            {
                CurrentActiveTabItem = selectedTab;
                var targetBrowserInstance = _tabBrowserMapping[selectedTab];

                _displayGrid.Children.Clear();
                _displayGrid.Children.Add(targetBrowserInstance);
                _omniboxControl.Text = targetBrowserInstance.Source?.ToString() ?? "";
            }
        }

        public void HandleTabCloseRequested(TabViewTabCloseRequestedEventArgs args, Action closeWindowCallback)
        {
            if (args.Item is TabViewItem targetTabItem && _tabBrowserMapping.ContainsKey(targetTabItem))
            {
                _tabBrowserMapping.Remove(targetTabItem);
                _tabViewControl.TabItems.Remove(targetTabItem);

                if (_tabViewControl.TabItems.Count == 0)
                {
                    closeWindowCallback?.Invoke();
                }
            }
        }

        public WebView2 GetActiveBrowserInstance()
        {
            if (CurrentActiveTabItem != null && _tabBrowserMapping.ContainsKey(CurrentActiveTabItem))
            {
                return _tabBrowserMapping[CurrentActiveTabItem];
            }
            return null;
        }
    }
}
