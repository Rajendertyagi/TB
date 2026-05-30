using System;
using System.Text.Json;
using System.Windows;
using Microsoft.Web.WebView2.Wpf;
using TB.Modules.Logging.Services;
using TB.Services;
using TB.Services.FeatureFlags;
using TB.ViewModels;

namespace TB;

public partial class MainWindow : Window
{
    private readonly BrowserViewModel _viewModel;
    private readonly IBrowserService _browserService;
    private readonly ITabManager _tabManager;
    private readonly IWebViewManager _webViewManager;
    private readonly IFeatureFlagService _featureFlagService;

    public MainWindow(
        BrowserViewModel viewModel,
        IBrowserService browserService,
        ITabManager tabManager,
        IWebViewManager webViewManager,
        IFeatureFlagService featureFlagService)
    {
        InitializeComponent();

        _viewModel = viewModel;
        _browserService = browserService;
        _tabManager = tabManager;
        _webViewManager = webViewManager;
        _featureFlagService = featureFlagService;

        DataContext = _viewModel;

        Loaded += MainWindow_Loaded;

        _tabManager.ActiveTabChanged +=
            async tab =>
            {
                var webView =
                    _webViewManager.Get(tab.Id);

                if (webView is null)
                {
                    webView =
                        _webViewManager.Create(
                            tab.Id);

                    _browserService.SetActiveBrowser(
                        webView);

                    BrowserHost.Content = webView;

                    await webView.EnsureCoreWebView2Async();

                    webView.CoreWebView2.WebMessageReceived +=
                        (_, args) =>
                        {
                            HandleWebMessage(
                                args.WebMessageAsJson);
                        };

                    TbLogger.WebViewInitialized();

                    webView.CoreWebView2.NavigationStarting +=
                        (_, args) =>
                        {
                            TbLogger.NavigationStarted(
                                args.Uri);
                        };

                    webView.CoreWebView2.NavigationCompleted +=
                        (_, args) =>
                        {
                            var url =
                                webView.Source?.ToString()
                                ?? "Unknown";

                            if (args.IsSuccess)
                            {
                                TbLogger.NavigationCompleted(
                                    url);
                            }
                            else
                            {
                                TbLogger.WebViewProcessFailed(
                                    args.WebErrorStatus.ToString());
                            }
                        };

                    webView.CoreWebView2.ProcessFailed +=
                        (_, args) =>
                        {
                            TbLogger.WebViewProcessFailed(
                                args.ProcessFailedKind.ToString());
                        };

                    _browserService.Navigate(
                        tab.Address);

                    return;
                }

                _browserService.SetActiveBrowser(
                    webView);

                BrowserHost.Content = webView;
            };
    }

    private async void MainWindow_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        var activeTab =
            _tabManager.ActiveTab;

        if (activeTab is null)
        {
            return;
        }

        var browser =
            _webViewManager.Create(
                activeTab.Id);

        BrowserHost.Content = browser;

        await browser.EnsureCoreWebView2Async();

        browser.CoreWebView2.WebMessageReceived +=
            (_, args) =>
            {
                HandleWebMessage(
                    args.WebMessageAsJson);
            };

        TbLogger.WebViewInitialized();

        browser.CoreWebView2.NavigationStarting +=
            (_, args) =>
            {
                TbLogger.NavigationStarted(
                    args.Uri);
            };

        browser.CoreWebView2.NavigationCompleted +=
            (_, args) =>
            {
                var url =
                    browser.Source?.ToString()
                    ?? "Unknown";

                if (args.IsSuccess)
                {
                    TbLogger.NavigationCompleted(
                        url);
                }
                else
                {
                    TbLogger.WebViewProcessFailed(
                        args.WebErrorStatus.ToString());
                }
            };

        browser.CoreWebView2.ProcessFailed +=
            (_, args) =>
            {
                TbLogger.WebViewProcessFailed(
                    args.ProcessFailedKind.ToString());
            };

        _browserService.Attach(browser);

        _browserService.SetActiveBrowser(
            browser);

        _browserService.Navigate(
            _viewModel.Address);
    }

    private void HandleWebMessage(
        string json)
    {
        try
        {
            var message =
                JsonSerializer.Deserialize<FeatureFlagMessage>(
                    json);

            if (message is null)
            {
                return;
            }

            if (!string.Equals(
                    message.Type,
                    "flag-toggle",
                    StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            _featureFlagService.SetEnabled(
                message.Name,
                message.Enabled);
        }
        catch (Exception ex)
        {
            TbLogger.Exception(
                ex);
        }
    }

    private void GoButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        _viewModel.NavigateCommand.Execute(
            null);
    }
}