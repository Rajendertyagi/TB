using System.Linq;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TB.Infrastructure.Bootstrap;
using TB.Infrastructure.Hosting;
using TB.Modules.Logging.Services;
using TB.Modules.Settings.Contracts;
using TB.Services;
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

        var settingsService =
            _host.Services.GetRequiredService<ISettingsService>();

        var tabManager =
            _host.Services.GetRequiredService<ITabManager>();

        try
        {
            var settings = settingsService.Load();

            if (settings.OpenTabs.Count > 0)
            {
                foreach (var address in settings.OpenTabs)
                {
                    tabManager.AddTab(address);
                }

                if (settings.ActiveTabIndex >= 0 &&
                    settings.ActiveTabIndex < tabManager.Tabs.Count)
                {
                    tabManager.SetActiveTab(
                        tabManager.Tabs[settings.ActiveTabIndex].Id);
                }
            }
        }
        catch
        {
            // Ignore corrupted settings and continue startup.
        }

        if (tabManager.Tabs.Count == 0)
        {
            tabManager.AddTab();
        }

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

            settings.ActiveTabIndex =
                tabManager.ActiveTab is null
                    ? 0
                    : tabManager.Tabs.IndexOf(tabManager.ActiveTab);

            settingsService.Save(settings);

            await _host.StopAsync();

            _host.Dispose();
        }

        base.OnExit(e);
    }
}