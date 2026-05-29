using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TB.Infrastructure.Bootstrap;
using TB.Infrastructure.Hosting;
using TB.Modules.Logging.Services;
using TB.Startup;

namespace TB;

public partial class App : Application
{
    private IHost? _host;

    protected override void OnStartup(StartupEventArgs e)
    {
        DirectoryBootstrapper.Initialize();

        LoggingConfigurator.Configure();

        _host = HostBuilderFactory.Create()
            .ConfigureServices(services =>
            {
                services.AddTbServices();
            })
            .Build();

        base.OnStartup(e);

        var window = _host.Services.GetRequiredService<MainWindow>();

        window.Show();
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (_host is not null)
        {
            await _host.StopAsync();
            _host.Dispose();
        }

        base.OnExit(e);
    }
}