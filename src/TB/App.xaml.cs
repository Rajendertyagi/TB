using System.Linq;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TB.Infrastructure.Bootstrap;
using TB.Infrastructure.Hosting;
using TB.Modules.Logging.Services;
using TB.Modules.Settings.Contracts;
using TB.Startup;
using TB.Services;

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
            var settingsService =
                _host.Services.GetRequiredService<ISettingsService>();

            var tabManager =
                _host.Services.GetRequiredService<ITabManager>();

            var settings = settingsService.Load();

            settings.OpenTabs =
                tabManager.Tabs
                    .Select(x => x.Address)
                    .ToList();

            settings.ActiveTab =
                tabManager.ActiveTab?.Address;

            settingsService.Save(settings);

            await _host.StopAsync();

            _host.Dispose();
        }

        base.OnExit(e);
    }
}