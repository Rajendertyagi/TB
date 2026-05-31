using Microsoft.Extensions.DependencyInjection;
using TB.Modules.Logging.Services;
using TB.Modules.Settings.Contracts;
using TB.Modules.Settings.Services;
using TB.Modules.Themes.Contracts;
using TB.Modules.Themes.Services;
using TB.Services;
using TB.Services.FeatureFlags;
using TB.Services.InternalPages;
using TB.ViewModels;

namespace TB.Startup;

public static class ServiceRegistration
{
    public static IServiceCollection AddTbServices(
        this IServiceCollection services)
    {
        LifecycleLogger.Created(
            nameof(ServiceRegistration));

        services.AddSingleton<
            ISettingsService,
            SettingsService>();

        CommandLogger.Completed(
            "ISettingsServiceRegistered");

        services.AddSingleton<
            IThemeService,
            ThemeService>();

        CommandLogger.Completed(
            "IThemeServiceRegistered");

        services.AddSingleton<
            IBookmarkService,
            BookmarkService>();

        CommandLogger.Completed(
            "IBookmarkServiceRegistered");

        services.AddSingleton<
            IFeatureFlagService,
            FeatureFlagService>();

        CommandLogger.Completed(
            "IFeatureFlagServiceRegistered");

        services.AddSingleton<
            IInternalPageService,
            InternalPageService>();

        CommandLogger.Completed(
            "IInternalPageServiceRegistered");

        services.AddSingleton<
            IBrowserService,
            WebView2BrowserService>();

        CommandLogger.Completed(
            "IBrowserServiceRegistered");

        services.AddSingleton<
            IWebViewManager,
            WebViewManager>();

        CommandLogger.Completed(
            "IWebViewManagerRegistered");

        services.AddSingleton<
            ITabManager,
            TabManager>();

        CommandLogger.Completed(
            "ITabManagerRegistered");

        services.AddSingleton<
            BrowserViewModel>();

        CommandLogger.Completed(
            "BrowserViewModelRegistered");

        services.AddSingleton<
            TabsViewModel>();

        CommandLogger.Completed(
            "TabsViewModelRegistered");

        services.AddSingleton<
            MainWindow>();

        CommandLogger.Completed(
            "MainWindowRegistered");

        LifecycleLogger.Initialized(
            nameof(ServiceRegistration));

        return services;
    }
}
