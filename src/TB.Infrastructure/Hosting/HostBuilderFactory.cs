using Microsoft.Extensions.Hosting;
using TB.Modules.Logging.Services;

namespace TB.Infrastructure.Hosting;

public static class HostBuilderFactory
{
    public static IHostBuilder Create()
    {
        LifecycleLogger.Created(
            nameof(HostBuilderFactory));

        var hostBuilder =
            Host.CreateDefaultBuilder();

        LifecycleLogger.Initialized(
            nameof(HostBuilderFactory));

        return hostBuilder;
    }
}
