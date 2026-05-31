using Microsoft.Extensions.DependencyInjection;
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
        services.AddSingleton<
            ISettingsService,
            SettingsService>();

        services.AddSingleton<
            IThemeService,
            ThemeService>();

        services.AddSingleton<
            IBookmarkService,
            BookmarkService>();

        services.AddSingleton<
            IFeatureFlagService,
            FeatureFlagService>();

        services.AddSingleton<
            IInternalPageService,
            InternalPageService>();

        services.AddSingleton<
            IBrowserService,
            WebView2BrowserService>();

        services.AddSingleton<
            IWebViewManager,
            WebViewManager>();

        services.AddSingleton<
            ITabManager,
            TabManager>();

        services.AddSingleton<
            BrowserViewModel>();

        services.AddSingleton<
            TabsViewModel>();

        services.AddSingleton<
            MainWindow>();

        return services;
    }
}
