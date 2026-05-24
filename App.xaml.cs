public partial class App : Application
{
    public App()
    {
        // 1. Initialize Logging
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Debug()
            .WriteTo.File(".tb_data/logs/log-.txt", rollingInterval: RollingInterval.Day)
            .CreateLogger();

        // 2. Initialize Database
        DatabaseHelper.Initialize();
        
        this.InitializeComponent();
    }
}
