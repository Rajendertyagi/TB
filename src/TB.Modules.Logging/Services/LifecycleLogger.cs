using Serilog;

namespace TB.Modules.Logging.Services;

public static class LifecycleLogger
{
    public static void Created(
        string typeName)
    {
        Log.Information(
            "Lifecycle.Created Type={Type}",
            typeName);
    }

    public static void Initialized(
        string typeName)
    {
        Log.Information(
            "Lifecycle.Initialized Type={Type}",
            typeName);
    }

    public static void Activated(
        string typeName)
    {
        Log.Information(
            "Lifecycle.Activated Type={Type}",
            typeName);
    }

    public static void Deactivated(
        string typeName)
    {
        Log.Information(
            "Lifecycle.Deactivated Type={Type}",
            typeName);
    }

    public static void Disposed(
        string typeName)
    {
        Log.Information(
            "Lifecycle.Disposed Type={Type}",
            typeName);
    }

    public static void Failed(
        string typeName,
        Exception exception)
    {
        Log.Error(
            exception,
            "Lifecycle.Failed Type={Type}",
            typeName);
    }
}