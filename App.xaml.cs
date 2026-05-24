using Microsoft.UI.Xaml;
using Serilog;
using System;

namespace TB
{
    public partial class App : Application
    {
        private Window m_window;
        public DatabaseService DbService { get; private set; }

        public App()
        {
            // Set required env variable for SingleFile deployment
            Environment.SetEnvironmentVariable("MICROSOFT_WINDOWSAPPRUNTIME_BASE_DIRECTORY", AppContext.BaseDirectory);

            this.InitializeComponent();

            Log.Logger = new LoggerConfiguration()
                .WriteTo.Debug()
                .WriteTo.File("logs/app-.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

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
