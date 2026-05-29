using Microsoft.Extensions.DependencyInjection;
using TB.ViewModels;

namespace TB.Startup;

public static class ServiceRegistration
{
    public static IServiceCollection AddTbServices(this IServiceCollection services)
    {
        services.AddSingleton<BrowserViewModel>();
        services.AddSingleton<MainWindow>();

        return services;
    }
}
