using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Web.WebView2.Core;
using Microsoft.Data.Sqlite;

namespace ModernWpfApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string DatabaseFileName = "LocalCache.db";
        private readonly string _dbPath;

        public MainWindow()
        {
            InitializeComponent();
            
            // Set up local app data path for SQLite and WebView2 user data
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string appDataFolder = Path.Combine(localAppData, "ModernWpfApp");
            Directory.CreateDirectory(appDataFolder);
            
            _dbPath = Path.Combine(appDataFolder, DatabaseFileName);

            // Initialize components asynchronously
            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                InitializeDatabase();
                await InitializeWebViewAsync();
                
                // Load an initial verified URL or local resource
                MainWebView.Source = new Uri("https://microsoft.com");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Initialization error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Initializes WebView2 with a dedicated, isolated user data folder.
        /// </summary>
        private async Task InitializeWebViewAsync()
        {
            if (MainWebView == null) return;

            // Set a secure local directory for browser cache and state
            string webViewCacheDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ModernWpfApp", "WebView2Profile");
            
            var environment = await CoreWebView2Environment.CreateAsync(userDataFolder: webViewCacheDir);
            await MainWebView.EnsureCoreWebView2Async(environment);

            // Configure modern security defaults
            MainWebView.CoreWebView2.Settings.IsPasswordAutofillEnabled = false;
            MainWebView.CoreWebView2.Settings.IsWebMessageEnabled = true;

            // Log navigation for cache syncing
            MainWebView.CoreWebView2.NavigationCompleted += WebView_NavigationCompleted;
        }

        /// <summary>
        /// Sets up the local SQLite architecture using Microsoft.Data.Sqlite.
        /// </summary>
        private void InitializeDatabase()
        {
            var connectionStringBuilder = new SqliteConnectionStringBuilder
            {
                DataSource = _dbPath,
                Mode = SqliteOpenMode.ReadWriteCreate
            };

            using (var connection = new SqliteConnection(connectionStringBuilder.ConnectionString))
            {
                connection.Open();

                string createTableQuery = @"
                    CREATE TABLE IF NOT EXISTS AppCache (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Key TEXT NOT NULL UNIQUE,
                        Value TEXT,
                        Timestamp DATETIME DEFAULT CURRENT_TIMESTAMP
                    );";

                using (var command = new SqliteCommand(createTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
                /// Saves navigation history or tracking metrics directly to the local database.
        /// </summary>
        private void WebView_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            if (!e.IsSuccess) return;

            string currentUrl = MainWebView.Source.ToString();

            using (var connection = new SqliteConnection($"Data Source={_dbPath}"))
            {
                connection.Open();

                string upsertQuery = @"
                    INSERT INTO AppCache (Key, Value, Timestamp) 
                    VALUES ('LastVisitedUrl', @Url, CURRENT_TIMESTAMP)
                    ON CONFLICT(Key) DO UPDATE SET 
                        Value = excluded.Value, 
                        Timestamp = CURRENT_TIMESTAMP;";

                using (var command = new SqliteCommand(upsertQuery, connection))
                {
                    command.Parameters.AddWithValue("@Url", currentUrl);
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
