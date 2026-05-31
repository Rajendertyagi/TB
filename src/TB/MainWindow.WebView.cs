using Microsoft.Web.WebView2.Wpf;
using TB.Modules.Logging.Services;

namespace TB;

public partial class MainWindow
{
    private void RegisterActiveTabChangedHandler()
    {
        _tabManager.ActiveTabChanged +=
            async tab =>
            {
                var webView =
                    _webViewManager.Get(
                        tab.Id);

                if (webView is null)
                {
                    webView =
                        _webViewManager.Create(
                            tab.Id);

                    _browserService.SetActiveBrowser(
                        webView);

                    BrowserHost.Content =
                        webView;

                    await InitializeWebViewAsync(
                        webView);

                    _browserService.Navigate(
                        tab.Address);

                    return;
                }

                _browserService.SetActiveBrowser(
                    webView);

                BrowserHost.Content =
                    webView;
            };
    }

    private async Task InitializeWebViewAsync(
        WebView2 webView)
    {
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
    }
}
