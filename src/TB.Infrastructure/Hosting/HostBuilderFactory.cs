using Microsoft.Extensions.Hosting;

namespace TB.Infrastructure.Hosting;

public static class HostBuilderFactory
{
    public static IHost Build()
    {
        return Host.CreateDefaultBuilder()
            .Build();
    }
}