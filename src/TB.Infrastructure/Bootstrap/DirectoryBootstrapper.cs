using System.IO;

namespace TB.Infrastructure.Bootstrap;

public static class DirectoryBootstrapper
{
    public static void Initialize()
    {
        Directory.CreateDirectory("Logs");
        Directory.CreateDirectory("Settings");
        Directory.CreateDirectory("UserData");
    }
}