using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using Microsoft.Data.Sqlite;

namespace TradingBrowser
{
    public sealed partial class MainWindow : Window
    {
        private readonly string _rootPortablePath;
        private readonly string _logDirectory;
        private readonly string _dbPath;
        
        // Dictionary tracking pairs to match TabItems directly with individual WebView2 browser frames
        private readonly Dictionary<TabViewItem, WebView2> _tabBrowserMapping = new Dictionary<TabViewItem, WebView2>();
        private TabViewItem _currentActiveTabItem = null;

        public MainWindow()
        {
            this.InitializeComponent();

            _rootPortablePath = AppDomain.CurrentDomain.BaseDirectory;
            _logDirectory = Path.Combine(_rootPortablePath, "logs");
            _dbPath = Path.Combine(_rootPortablePath, "userdata.db");
            Directory.CreateDirectory(_logDirectory);
            
            // Turn on borderless layout mapping and designate Tier 1 as the draggable track handle
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);

            // Hook layout events explicitly
            MainBrowserTabs.AddTabButtonClick += OnNewTabRequested;
            MainBrowserTabs.SelectionChanged += OnTabSelectionChanged;
            MainBrowserTabs.TabCloseRequested += OnTabCloseRequested;
            
            Omnibox.QuerySubmitted += OnOmniboxSubmitted;
            BackButton.Click += (s, e) => { GetActiveBrowser()?.GoBack(); };
            ForwardButton.Click += (s, e) => { GetActiveBrowser()?.GoForward(); };
            RefreshButton.Click += (s, e) => { GetActiveBrowser()?.CoreWebView2?.Reload(); };

            InitializePersistenceEngine();
            
            // Clear out the pre-compiler placeholder tab safely before initializing real tabs
            MainBrowserTabs.TabItems.Clear();
            
            // Create your very first startup workspace tab automatically on boot
            CreateNewTabContext("TradingView Workspace", "https://www.tradingview.com/chart/");
        }

        private void InitializePersistenceEngine()
        {
            try
            {
                using var conn = new SqliteConnection($"Data Source={_dbPath}");
                conn.Open();
                string createTables = "CREATE TABLE IF NOT EXISTS store (key TEXT PRIMARY KEY, val TEXT);";
                using var cmd = new SqliteCommand(createTables, conn);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                WriteLog("Global Errors", $"SQLite Init Failure: {ex.Message}");
            }
        }

        private void CreateNewTabContext(string tabTitle, string targetUrl)
        {
            // Create a real, visible visual tab pill item in the TabView control bar
            var newTabItem = new TabViewItem
            {
                Header = tabTitle,
                IconSource = new SymbolIconSource { Symbol = Symbol.Document }
            };

            // Instantiate a completely isolated dedicated browser container instance for this specific tab context
            var webBrowserInstance = new WebView2
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };

            // Save the connection pairing into our mapping framework index layout 
            _tabBrowserMapping.Add(newTabItem, webBrowserInstance);
            MainBrowserTabs.TabItems.Add(newTabItem);
            
            // Auto-focus selection onto the newly built workspace tab
            MainBrowserTabs.SelectedItem = newTabItem;

            DispatcherQueue.TryEnqueue(async () =>
            {
                try
                {
                    string cachePath = Path.Combine(_rootPortablePath, "WebViewCache");
                    var options = new CoreWebView2EnvironmentOptions();
                    
                    var env = await CoreWebView2Environment.CreateWithOptionsAsync(
                        browserExecutableFolder: string.Empty, 
                        userDataFolder: cachePath, 
                        options: options
                    );
                    
                    await webBrowserInstance.EnsureCoreWebView2Async(env);
                    webBrowserInstance.CoreWebView2.Settings.IsWebMessageEnabled = true;
                    
                    // Capture internal frame routes to dynamically sync the wide address bar text
                    webBrowserInstance.CoreWebView2.SourceChanged += (s, e) =>
                    {
                        if (MainBrowserTabs.SelectedItem == newTabItem)
                        {
                            Omnibox.Text = webBrowserInstance.Source.ToString();
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
                    WriteLog("Global Errors", $"WebView instantiation error: {ex.Message}");
                }
            });
        }

        private void OnTabSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MainBrowserTabs.SelectedItem is TabViewItem selectedTab && _tabBrowserMapping.ContainsKey(selectedTab))
            {
                _currentActiveTabItem = selectedTab;
                var targetBrowserInstance = _tabBrowserMapping[selectedTab];

                // Swap out viewport visibility blocks to project the focused workspace canvas content frame
                TabContentDisplayGrid.Children.Clear();
                TabContentDisplayGrid.Children.Add(targetBrowserInstance);

                // Update the address entry string field display value 
                Omnibox.Text = targetBrowserInstance.Source?.ToString() ?? "";
            }
        }

        private void OnNewTabRequested(TabView sender, object args)
        {
            // Creates and registers a clean visual workspace tab upon clicking '+'
            CreateNewTabContext("New Tab", "https://www.tradingview.com/chart/");
        }

        private void OnTabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
        {
            if (args.Item is TabViewItem targetTabItem && _tabBrowserMapping.ContainsKey(targetTabItem))
            {
                var browserToDispose = _tabBrowserMapping[targetTabItem];
                _tabBrowserMapping.Remove(targetTabItem);
                MainBrowserTabs.TabItems.Remove(targetTabItem);

                // Safety checks for closing down the last remaining workspace window frame anchor
                if (MainBrowserTabs.TabItems.Count == 0)
                {
                    this.Close();
                }
            }
        }

        private WebView2 GetActiveBrowser()
        {
            if (_currentActiveTabItem != null && _tabBrowserMapping.ContainsKey(_currentActiveTabItem))
            {
                return _tabBrowserMapping[_currentActiveTabItem];
            }
            return null;
        }

        private void OnOmniboxSubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            string rawInput = sender.Text.Trim();
            var activeBrowser = GetActiveBrowser();
            if (string.IsNullOrEmpty(rawInput) || activeBrowser == null) return;

            string targetUrl;
            if (rawInput.StartsWith("http://") || rawInput.StartsWith("https://") || (rawInput.Contains(".") && !rawInput.Contains(" ")))
            {
                targetUrl = rawInput.StartsWith("http") ? rawInput : "https://" + rawInput;
            }
            else
            {
                targetUrl = "https://www.google.com/search?q=" + Uri.EscapeDataString(rawInput);
            }

            activeBrowser.Source = new Uri(targetUrl);
        }

        private void WriteLog(string trackCategory, string dataLogged)
        {
            try
            {
                string logLine = $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}] [{trackCategory}] {dataLogged}{Environment.NewLine}";
                string todayFile = Path.Combine(_logDirectory, $"trading_session_{DateTime.UtcNow:yyyyMMdd}.log");
                File.AppendAllText(todayFile, logLine);
            }
            catch { /* Protection anchor locks */ }
        }
    }
}
