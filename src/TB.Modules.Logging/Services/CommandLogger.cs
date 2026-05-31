using Serilog;

namespace TB.Modules.Logging.Services;

public static class CommandLogger
{
    public static void Requested(
        string commandName)
    {
        Log.Information(
            "Command.Requested Name={Command}",
            commandName);
    }

    public static void Started(
        string commandName)
    {
        Log.Information(
            "Command.Started Name={Command}",
            commandName);
    }

    public static void Completed(
        string commandName)
    {
        Log.Information(
            "Command.Completed Name={Command}",
            commandName);
    }

    public static void Success(
        string commandName)
    {
        Log.Information(
            "Command.Success Name={Command}",
            commandName);
    }

    public static void Warning(
        string commandName,
        string message)
    {
        Log.Warning(
            "Command.Warning Name={Command} Message={Message}",
            commandName,
            message);
    }

    public static void Cancelled(
        string commandName)
    {
        Log.Warning(
            "Command.Cancelled Name={Command}",
            commandName);
    }

    public static void Failed(
        string commandName,
        Exception exception)
    {
        Log.Error(
            exception,
            "Command.Failed Name={Command}",
            commandName);
    }
}