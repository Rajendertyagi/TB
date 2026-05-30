using System.IO;
using System.Linq;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using TB.Infrastructure.Bootstrap;
using TB.Infrastructure.Hosting;
using TB.Modules.Logging.Services;
using TB.Modules.Settings.Contracts;
using TB.Services;
using TB.Startup;

namespace TB;

public partial class App : Application
{
    private const string RunningFlagFile = "Settings/running.flag";

    private IHost? _host;

    protected override void OnStartup(StartupEventArgs e)
    {
        DirectoryBootstrapper.Initialize();

        LoggingConfigurator.Configure();

        if (File.Exists(RunningFlagFile))
        {
            Log.Warning(
                "Previous RTBrowser session ended unexpectedly. Crash recovery activated.");
        }

        File.WriteAllText(
            RunningFlagFile,
            DateTime.UtcNow.ToString("O"));

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
        catch (Exception ex)
        {
            Log.Error(
                ex,
                "Failed to restore session. Starting with default tab.");
        }

        if (tabManager.Tabs.Count == 0)
        {
            tabManager.AddTab();
        }

        base.OnStartup(e);

        var window = _host.Services.GetRequiredService<MainWindow>();

        window.Show();

        Log.Information("RTBrowser started successfully.");
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        try
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

            if (File.Exists(RunningFlagFile))
            {
                File.Delete(RunningFlagFile);
            }

            Log.Information("RTBrowser exited cleanly.");
        }
        catch (Exception ex)
        {
            Log.Error(
                ex,
                "Error during shutdown.");
        }

        base.OnExit(e);
    }
}