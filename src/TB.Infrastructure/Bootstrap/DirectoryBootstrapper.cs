using System.IO;
using TB.Modules.Logging.Services;

namespace TB.Infrastructure.Bootstrap;

public static class DirectoryBootstrapper
{
    public static void Initialize()
    {
        LifecycleLogger.Created(
            nameof(DirectoryBootstrapper));

        Directory.CreateDirectory(
            "Logs");

        CommandLogger.Completed(
            "LogsDirectoryCreated");

        Directory.CreateDirectory(
            "Settings");

        CommandLogger.Completed(
            "SettingsDirectoryCreated");

        Directory.CreateDirectory(
            "UserData");

        CommandLogger.Completed(
            "UserDataDirectoryCreated");

        File.AppendAllText(
            "Logs/app.log",
            string.Empty);

        CommandLogger.Completed(
            "LogFileVerified");

        LifecycleLogger.Initialized(
            nameof(DirectoryBootstrapper));
    }
}
