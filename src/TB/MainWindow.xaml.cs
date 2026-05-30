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

    public MainWindow(
        BrowserViewModel viewModel,
        IBrowserService browserService)
    {
        InitializeComponent();

        _viewModel = viewModel;
        _browserService = browserService;

        DataContext = _viewModel;

        Loaded += MainWindow_Loaded;
    }

    private async void MainWindow_Loaded(
        object sender,
        RoutedEventArgs e)
    {
        var browser = new WebView2();

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
