using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using TB.Services.Downloads;
using TB.Helpers;
using TB.Infrastructure;
using TB.Input;
using TB.Services;
using TB.Services.Interfaces;
using TB.ViewModels;
using System.Threading.Tasks;

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

        // 1. UI Thread Exceptions
        UnhandledException += (_, e) =>
        {
            try
            {
                Logger.Error($"[UI THREAD CRASH] {e.Exception?.GetType().FullName}: {e.Exception?.Message}\n{e.Exception?.StackTrace}");

                // Prevent the app from silently crashing
                e.Handled = true;

                ShowCrashDialog("UI Error", e.Exception?.Message ?? "An unexpected UI error occurred.");
            }
            catch
            {
            }
        };

        // 2. Background Thread Exceptions (Fatal, but we log it before the OS kills the app)
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            try
            {
                Logger.Error($"[FATAL BACKGROUND CRASH] {e.ExceptionObject}");
            }
            catch
            {
            }
        };

        // 3. Unobserved Async Task Exceptions (e.g., _ = SomeAsyncMethod())
        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            try
            {
                Logger.Error($"[ASYNC TASK CRASH] {e.Exception?.InnerException?.Message ?? e.Exception?.Message}\n{e.Exception?.StackTrace}");

                // Prevent the app from crashing
                e.SetObserved();

                ShowCrashDialog("Background Task Error", "A background process failed. Your data is safe, but some features may not have updated.");
            }
            catch
            {
            }
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
            Logger.Info($"Theme loaded. bgAppBrush exists = {appResources.ContainsKey("bgAppBrush")}, Theme dictionaries = {appResources.ThemeDictionaries.Count}");

            var window = _services.GetRequiredService<MainWindow>();

            // FIX: Assign MainWindow EARLY so crash handlers can use its XamlRoot if OnLaunched throws
            MainWindow = window;

            window.Activate();

            if (window.Content is FrameworkElement root)
            {
                var settings = _services.GetRequiredService<ISettingsService>();
                var themeMode = settings.Get("theme-mode", "dark") ?? "dark";
                root.RequestedTheme = themeMode == "light" ? ElementTheme.Light : ElementTheme.Dark;
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
        // We must dispatch to the UI thread to show XAML dialogs
        MainWindow?.DispatcherQueue?.TryEnqueue(async () =>
        {
            try
            {
                var dialog = new ContentDialog
                {
                    Title = title,
                    Content = $"TB Browser caught an error to prevent a crash.\n\nDetails: {message}",
                    CloseButtonText = "OK",
                    XamlRoot = MainWindow?.Content?.XamlRoot // Required in WinUI 3
                };

                if (dialog.XamlRoot != null)
                {
                    await dialog.ShowAsync();
                }
            }
            catch (Exception dialogEx)
            {
                // If the dialog itself fails to render, just log it
                Logger.Warning($"Failed to show crash dialog: {dialogEx.Message}");
            }
        });
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        var basePath = AppDomain.CurrentDomain.BaseDirectory;

        services.AddSingleton<ISettingsService>(
            _ => new SettingsService(basePath));

        services.AddSingleton<IDownloadService>(
            _ => new DownloadService(basePath));

        services.AddSingleton<IThemeService>(
            sp => new ThemeService(
                basePath,
                sp.GetRequiredService<ISettingsService>()));

        services.AddSingleton<CommandRegistry>(
            sp => new CommandRegistry(
                new Lazy<ITabManager>(() => sp.GetRequiredService<ITabManager>()),
                new Lazy<INavigationService>(() => sp.GetRequiredService<INavigationService>())));

        services.AddSingleton<KeyboardShortcutHandler>();

        services.AddSingleton<ITabManager>(
            sp => new TabManager(
                basePath,
                sp.GetRequiredService<IThemeService>(),
                sp.GetRequiredService<ISettingsService>(),
                sp.GetRequiredService<IDownloadService>(),
                sp.GetRequiredService<KeyboardShortcutHandler>()));

        services.AddSingleton<INavigationService, NavigationService>();

        services.AddSingleton<ChromeViewModel>();
        services.AddSingleton<MainViewModel>();
        services.AddTransient<TabItemViewModel>();

        services.AddTransient<MainWindow>();
    }


}