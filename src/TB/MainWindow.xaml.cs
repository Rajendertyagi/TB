using System;
using System.Windows;
using Microsoft.Web.WebView2.Wpf;
using TB.Modules.Logging.Services;
using TB.Services;
using TB.ViewModels;

namespace TB;

public partial class MainWindow : Window
{
    private readonly BrowserViewModel _viewModel;
    private readonly IBrowserService _browserService;
    private readonly ITabManager _tabManager;
    private readonly IWebViewManager _webViewManager;

    public MainWindow(
        BrowserViewModel viewModel,
        IBrowserService browserService,
        ITabManager tabManager,
        IWebViewManager webViewManager)
    {
        InitializeComponent();

        _viewModel = viewModel;
        _browserService = browserService;
        _tabManager = tabManager;
        _webViewManager = webViewManager;

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

    private void GoButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        _viewModel.NavigateCommand.Execute(
            null);
    }
}
