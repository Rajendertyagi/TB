using Serilog;

namespace TB.Modules.Logging.Services;

public static class ApplicationLogger
{
    public static void Startup()
    {
        Log.Information(
            "Application.Startup");
    }

    public static void Shutdown()
    {
        Log.Information(
            "Application.Shutdown");
    }

    public static void CrashDetected()
    {
        Log.Warning(
            "Application.CrashDetected");
    }

    public static void UnhandledException(
        Exception exception)
    {
        Log.Error(
            exception,
            "Application.UnhandledException");
    }

    public static void Started(
        string componentName)
    {
        Log.Information(
            "Application.ComponentStarted Component={Component}",
            componentName);
    }

    public static void Stopped(
        string componentName)
    {
        Log.Information(
            "Application.ComponentStopped Component={Component}",
            componentName);
    }
}