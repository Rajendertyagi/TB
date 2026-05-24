using Microsoft.UI.Xaml;
using Serilog;

namespace TB
{
    public partial class App : Application
    {
        private Window m_window;
        public DatabaseService DbService { get; private set; }

        public App()
        {
            this.InitializeComponent();

            // Initialize Logging
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Debug()
                .WriteTo.File("logs/app-.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            Log.Information("Application starting up.");

            // Initialize Database
            DbService = new DatabaseService();
            DbService.Initialize();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            m_window = new MainWindow();
            m_window.Activate();
        }
    }
}
