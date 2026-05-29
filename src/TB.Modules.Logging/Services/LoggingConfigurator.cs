using Serilog;

namespace TB.Modules.Logging.Services;

public static class LoggingConfigurator
{
    public static void Configure()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File(
                path: "Logs/app.log",
                rollingInterval: RollingInterval.Day,
                shared: true)
            .CreateLogger();

        Log.Information("Logging initialized.");
    }
}