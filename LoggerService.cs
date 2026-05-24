using Serilog;

namespace TradingBrowser
{
    public static class LoggerService
    {
        /// <summary>
        /// Configures the logging heartbeat.
        /// Call this once at the very start of the application lifecycle.
        /// </summary>
        public static void Initialize()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Debug() // Optimized for performance: writes to the VS Debug Output
                .CreateLogger();
        }

        public static void Info(string message) => Log.Information(message);
        
        public static void Error(string message, System.Exception ex) => Log.Error(ex, message);
    }
}
