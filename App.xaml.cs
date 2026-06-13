using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Threading.Tasks;
using TB.Services.Downloads;
using TB.Helpers;
using TB.Infrastructure;
using TB.Input;
using TB.Services;
using TB.Services.Interfaces;
using TB.ViewModels;

using Application = Microsoft.UI.Xaml.Application;

namespace TB;

public partial class App : Application
{
    private IServiceProvider? _services;

    public static IServiceProvider Services =>
        (Current as App)?._services
        ?? throw new InvalidOperationException("Services not initialized");

    public static Window? MainWindow { get; private set; }

    public App()
    {
        InitializeComponent();

        UnhandledException += (_, e) =>
        {
            try
            {
                Logger.Error(
                    $"WinUI UnhandledException: {e.Exception?.GetType().FullName}: {e.Exception?.Message}\n{e.Exception?.StackTrace}");
                e.Handled = true;
                ShowCrashDialog("UI Error", e.Exception?.Message ?? "An unexpected UI error occurred.");
            }
            catch { }
        };

        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            try { Logger.Error($"AppDomain UnhandledException: {e.ExceptionObject}"); }
            catch { }
        };

        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            try { Logger.Error($"TaskScheduler UnobservedTaskException: {e.Exception}"); }
            catch { }
            e.SetObserved();
        };
    }

    protected override async void OnLaunched(LaunchActivatedEventArgs args)
    {
        try
        {
            Logger.Initialize(AppDomain.CurrentDomain.BaseDirectory);

            var services = new ServiceCollection();
            ConfigureServices(services);

            _services = services.BuildServiceProvider();

            var themeService = _services.GetRequiredService<IThemeService>();
            await themeService.ReloadThemeAsync();
            themeService.ApplyXamlResources();

            var appResources = Application.Current.Resources;
            Logger.Info(
                $"Theme loaded. bgAppBrush exists = {appResources.ContainsKey("bgAppBrush")}, Theme dictionaries = {appResources.ThemeDictionaries.Count}");

            var window = _services.GetRequiredService<MainWindow>();

            MainWindow = window;
            window.Activate();

            if (window.Content is FrameworkElement root)
            {
                root.RequestedTheme = ElementTheme.Default;
            }

            themeService.NotifyThemeChanged();
        }
        catch (Exception ex)
        {
            Logger.Error($"Application startup failed: {ex}");
            throw;
        }
    }

    private void ShowCrashDialog(string title, string message)
    {
        MainWindow?.DispatcherQueue?.TryEnqueue(async () =>
        {
            try
            {
                var dialog = new ContentDialog
                {
                    Title = title,
                    Content = $"TB Browser caught an error to prevent a crash.\n\nDetails: {message}",
                    CloseButtonText = "OK",
                    XamlRoot = MainWindow?.Content?.XamlRoot
                };

                if (dialog.XamlRoot != null)
                {
                    await dialog.ShowAsync();
                }
            }
            catch (Exception dialogEx)
            {
                Logger.Warning($"Failed to show crash dialog: {dialogEx.Message}");
            }
        });
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        var basePath = AppDomain.CurrentDomain.BaseDirectory;

        services.AddSingleton<ISettingsService>(_ => new SettingsService(basePath));
        services.AddSingleton<IDownloadService>(_ => new DownloadService(basePath));
        services.AddSingleton<IFlagService>(
            sp => new FlagService(sp.GetRequiredService<ISettingsService>()));

        services.AddSingleton<IThemeService>(
            sp => new ThemeService(basePath, sp.GetRequiredService<ISettingsService>()));

        services.AddSingleton<CommandRegistry>(
            sp => new CommandRegistry(
                new Lazy<ITabManager>(() => sp.GetRequiredService<ITabManager>()),
                new Lazy<INavigationService>(() => sp.GetRequiredService<INavigationService>())));

        // ✅ Handler depends only on ITabManager and INavigationService (both registered)
        services.AddSingleton<KeyboardShortcutHandler>();

        // ✅ TabManager must NOT depend on KeyboardShortcutHandler
        services.AddSingleton<ITabManager>(
    sp => new TabManager(
        basePath,
        sp.GetRequiredService<IThemeService>(),
        sp.GetRequiredService<ISettingsService>(),
        sp.GetRequiredService<IDownloadService>(),
        sp.GetRequiredService<KeyboardShortcutHandler>(),
        sp.GetRequiredService<IFlagService>()));

        services.AddSingleton<INavigationService, NavigationService>();

        services.AddSingleton<ChromeViewModel>();
        services.AddSingleton<MainViewModel>();

        services.AddTransient<MainWindow>();
    }
}