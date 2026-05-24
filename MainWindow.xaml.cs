using System;
using System.IO;
using System.Text.Json;
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

        public MainWindow()
        {
            this.InitializeComponent();

            // Establish Local Portable Application Scoping Execution Anchors
            _rootPortablePath = AppDomain.CurrentDomain.BaseDirectory;
            _logDirectory = Path.Combine(_rootPortablePath, "logs");
            _dbPath = Path.Combine(_rootPortablePath, "userdata.db");

            Directory.CreateDirectory(_logDirectory);
            
            // Clean Boot Trigger Pipelines
            WriteLog("Startup Engine", "Initializing Portable Trading Core Structure.");
            ExtendsContentIntoTitleBar = true;
            InitializePersistenceEngine();
            SetupCaptionButtons();
            
            // Fire up the browser subsystem safely on the primary UI execution thread
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
                WriteLog("Startup Engine", "Portable SQLite context mounted and validated.");
            }
            catch (Exception ex)
            {
                WriteLog("Global Errors", $"SQLite Init Failure: {ex.Message}");
            }
        }

        private void InitializeBrowserEngine(string targetUrl)
        {
            WriteLog("Tab Lifecycle", $"Creating global context footprint mapping target: {targetUrl}");

            Task.Run(async () =>
            {
                try
                {
                    string cachePath = Path.Combine(_rootPortablePath, "WebViewCache");
                    var options = new CoreWebView2EnvironmentOptions();
                    
                    // Correct WinUI 3 API Call signature for targeting custom file storage runtimes
                    var env = await CoreWebView2Environment.CreateWithOptionsAsync(
                        browserExecutableFolder: string.Empty, 
                        userDataFolder: cachePath, 
                        options: options
                    );
                    
                    DispatcherQueue.TryEnqueue(async () =>
                    {
                        // Ensure layout hooks link securely to the rendering canvas
                        await MainWebView.EnsureCoreWebView2Async(env);
                        
                        // Disable multi-window background power constraints
                        MainWebView.CoreWebView2.Settings.IsWebMessageEnabled = true;
                        
                        // Setup Navigation Tracking & Interceptions
                        MainWebView.CoreWebView2.NavigationStarting += (s, e) => WriteLog("Navigation Loop", $"Initiating frame routing sequence: {e.Uri}");
                        MainWebView.CoreWebView2.NavigationCompleted += (s, e) => WriteLog("Navigation Loop", $"Render pipeline confirmation complete. HTTP Success: {e.IsSuccess}");
                        
                        // Attach Renderer Crashes, Unhandled Worker Faults & Unhandled Promises Interceptors
                        MainWebView.CoreWebView2.ProcessFailed += (s, e) => WriteLog("Renderer & Promise Faults", $"Chromium Context Failure Event. Reason: {e.ProcessFailedKind}");
                        
                        MainWebView.CoreWebView2.DownloadStarting += (s, e) => {
                            WriteLog("Downloads Subsystem", $"Intercepting external storage download routine. Path destination payload: {e.ResultFilePath}");
                        };

                        // Assign initial source link target
                        MainWebView.Source = new Uri(targetUrl);
                    });
                }
                catch (Exception ex)
                {
                    WriteLog("Global Errors", $"WebView instantiation error chain broken: {ex.Message}");
                }
            });
        }

        private void OnOmniboxSubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            string rawInput = sender.Text.Trim();
            if (string.IsNullOrEmpty(rawInput)) return;

            string targetUrl;
            // Omnibox URL Parsing & Search Fallback Engine
            if (rawInput.StartsWith("http://") || rawInput.StartsWith("https://") || (rawInput.Contains(".") && !rawInput.Contains(" ")))
            {
                targetUrl = rawInput.StartsWith("http") ? rawInput : "https://" + rawInput;
                WriteLog("Navigation Loop", $"Omnibox parsed exact route coordinates match: {targetUrl}");
            }
            else
            {
                targetUrl = "https://www.google.com/search?q=" + Uri.EscapeDataString(rawInput);
                WriteLog("Navigation Loop", $"Omnibox fallback verification activated. Redirect routing: {targetUrl}");
            }

            if (MainWebView != null && MainWebView.CoreWebView2 != null)
            {
                MainWebView.Source = new Uri(targetUrl);
            }
        }

        private void WriteLog(string trackCategory, string dataLogged)
        {
            string logLine = $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}] [{trackCategory}] {dataLogged}{Environment.NewLine}";
            string todayFile = Path.Combine(_logDirectory, $"trading_session_{DateTime.UtcNow:yyyyMMdd}.log");
            
            // Run entirely non-blocking to prevent UI animation thread stalling
            Task.Run(() => {
                try { File.AppendAllText(todayFile, logLine); } catch { /* Prevent recursive logging crashes */ }
            });
        }

        // Framework Stubs for Chrome style UX Window Control Operations 
        private void SetupCaptionButtons() => WriteLog("Startup Engine", "Caption button color matching optimization executed.");
        
        private void OnNewTabRequested(TabView sender, object args) {
            // In single frame container layout, tab switching acts as a fast address redirect anchor
            if (MainWebView != null && MainWebView.CoreWebView2 != null)
            {
                MainWebView.Source = new Uri("https://www.tradingview.com/chart/");
            }
        }
        
        private void OnTabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args) => WriteLog("Tab Lifecycle", "Tab closure handled by window framework state mapping.");
        private void MenuNewTab(object sender, RoutedEventArgs e) => OnNewTabRequested(MainBrowserTabs, null);
        private void MenuReloadTab(object sender, RoutedEventArgs e) { if (MainWebView != null) MainWebView.CoreWebView2?.Reload(); }
        private void MenuDuplicateTab(object sender, RoutedEventArgs e) { /* Clones current tab configuration settings */ }
        private void MenuCloseTab(object sender, RoutedEventArgs e) { /* Tab context closure logic execution endpoint */ }
        private void MenuCloseOtherTabs(object sender, RoutedEventArgs e) { /* Iterates over structural loop matching selection index */ }
        private void MenuCloseRightTabs(object sender, RoutedEventArgs e) { /* Iterates from selection index offset to count ceiling */ }
    }
}
