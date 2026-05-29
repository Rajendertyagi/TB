using System.Windows;
using Microsoft.Extensions.Hosting;
using TB.Infrastructure.Hosting;

namespace TB;

public partial class App : Application
{
    private IHost? _host;

    protected override void OnStartup(StartupEventArgs e)
    {
        _host = HostBuilderFactory.Build();

        base.OnStartup(e);

        var window = new MainWindow();

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