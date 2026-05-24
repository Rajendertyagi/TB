using System;
using System.IO;
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
        private bool _isWebViewInitialized = false;
        private WebView2 _activeBrowserInstance;

        public MainWindow()
        {
            this.InitializeComponent();

            // Establish Local Portable Application Scoping Execution Anchors
            _rootPortablePath = AppDomain.CurrentDomain.BaseDirectory;
            _logDirectory = Path.Combine(_rootPortablePath, "logs");
            _dbPath = Path.Combine(_rootPortablePath, "userdata.db");

            Directory.CreateDirectory(_logDirectory);
            
            WriteLog("Startup Engine", "Initializing Portable Core Web UI Canvas Engine.");
            ExtendsContentIntoTitleBar = true;
            
            // Explicitly hook layout events via pure C# compilation roots
            MainBrowserTabs.AddTabButtonClick += OnNewTabRequested;
            MainBrowserTabs.TabCloseRequested += OnTabCloseRequested;
            Omnibox.QuerySubmitted += OnOmniboxSubmitted;

            InitializePersistenceEngine();
            this.Activated += MainWindow_Activated;
        }

        private void MainWindow_Activated(object sender, WindowActivatedEventArgs id)
        {
            if (!_isWebViewInitialized)
            {
                _isWebViewInitialized = true;
                InitializeBrowserEngine("https://www.tradingview.com/chart/");
            }
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

        private void InitializeBrowserEngine(string targetUrl)
        {
            _activeBrowserInstance = new WebView2
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch
            };

            // Inject instance explicitly inside structural Grid container tree framework
            TabContentDisplayGrid.Children.Clear();
            TabContentDisplayGrid.Children.Add(_activeBrowserInstance);

            Task.Run(async () =>
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
                    
                    DispatcherQueue.TryEnqueue(async () =>
                    {
                        await _activeBrowserInstance.EnsureCoreWebView2Async(env);
                        _activeBrowserInstance.CoreWebView2.Settings.IsWebMessageEnabled = true;
                        
                        _activeBrowserInstance.CoreWebView2.NavigationStarting += (s, e) => WriteLog("Navigation Loop", $"Routing: {e.Uri}");
                        _activeBrowserInstance.CoreWebView2.NavigationCompleted += (s, e) => WriteLog("Navigation Loop", $"Loaded. Status: {e.IsSuccess}");
                        
                        _activeBrowserInstance.Source = new Uri(targetUrl);
                    });
                }
                catch (Exception ex)
                {
                    WriteLog("Global Errors", $"WebView instantiation error: {ex.Message}");
                }
            });
        }

        private void OnOmniboxSubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            string rawInput = sender.Text.Trim();
            if (string.IsNullOrEmpty(rawInput) || _activeBrowserInstance == null) return;

            string targetUrl;
            if (rawInput.StartsWith("http://") || rawInput.StartsWith("https://") || (rawInput.Contains(".") && !rawInput.Contains(" ")))
            {
                targetUrl = rawInput.StartsWith("http") ? rawInput : "https://" + rawInput;
            }
            else
            {
                targetUrl = "https://www.google.com/search?q=" + Uri.EscapeDataString(rawInput);
            }

            _activeBrowserInstance.Source = new Uri(targetUrl);
        }

        private void WriteLog(string trackCategory, string dataLogged)
        {
            try
            {
                string logLine = $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}] [{trackCategory}] {dataLogged}{Environment.NewLine}");
                string todayFile = Path.Combine(_logDirectory, $"trading_session_{DateTime.UtcNow:yyyyMMdd}.log");
                File.AppendAllText(todayFile, logLine);
            }
            catch { /* System IO protection anchors */ }
        }

        private void OnNewTabRequested(TabView sender, object args)
        {
            if (_activeBrowserInstance != null)
            {
                _activeBrowserInstance.Source = new Uri("https://www.tradingview.com/chart/");
            }
        }
        
        private void OnTabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args) { }
    }
}
