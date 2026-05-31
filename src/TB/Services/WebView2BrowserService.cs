using Microsoft.Web.WebView2.Wpf;
using TB.Modules.Logging.Services;
using TB.Services.InternalPages;

namespace TB.Services;

public sealed class WebView2BrowserService : IBrowserService
{
    private readonly IInternalPageService _internalPageService;

    private WebView2? _browser;

    public WebView2BrowserService(
        IInternalPageService internalPageService)
    {
        LifecycleLogger.Created(
            nameof(WebView2BrowserService));

        _internalPageService =
            internalPageService;

        LifecycleLogger.Initialized(
            nameof(WebView2BrowserService));
    }

    public void Attach(WebView2 browser)
    {
        _browser = browser;

        TbLogger.WebViewAttached();

        LifecycleLogger.Activated(
            nameof(WebView2BrowserService));
    }

    public void SetActiveBrowser(WebView2 browser)
    {
        _browser = browser;

        CommandLogger.Completed(
            "SetActiveBrowser");
    }

    public void Navigate(string url)
    {
        CommandLogger.Requested(
            "Navigate");

        TbLogger.NavigationRequested(
            url);

        try
        {
            if (_browser?.CoreWebView2 is null)
            {
                CommandLogger.Warning(
                    "Navigate",
                    "Browser was not initialized");

                return;
            }

            if (string.IsNullOrWhiteSpace(
                    url))
            {
                CommandLogger.Warning(
                    "Navigate",
                    "Url was empty");

                return;
            }

            if (_internalPageService.TryGetPage(
                    url,
                    out var html))
            {
                CommandLogger.Completed(
                    "InternalPageNavigation");

                _browser.CoreWebView2.NavigateToString(
                    html);

                return;
            }

            if (!url.StartsWith(
                    "http://",
                    StringComparison.OrdinalIgnoreCase) &&
                !url.StartsWith(
                    "https://",
                    StringComparison.OrdinalIgnoreCase))
            {
                url =
                    $"https://{url}";

                CommandLogger.Completed(
                    "UrlNormalized");
            }

            _browser.CoreWebView2.Navigate(
                url);

            CommandLogger.Completed(
                "Navigate");
        }
        catch (Exception ex)
        {
            TbLogger.NavigationFailed(
                url,
                ex);

            CommandLogger.Failed(
                "Navigate",
                ex);

            throw;
        }
    }

    public void GoBack()
    {
        CommandLogger.Requested(
            "GoBack");

        if (_browser?.CoreWebView2?.CanGoBack == true)
        {
            _browser.CoreWebView2.GoBack();

            CommandLogger.Completed(
                "GoBack");

            return;
        }

        CommandLogger.Warning(
            "GoBack",
            "No back history available");
    }

    public void GoForward()
    {
        CommandLogger.Requested(
            "GoForward");

        if (_browser?.CoreWebView2?.CanGoForward == true)
        {
            _browser.CoreWebView2.GoForward();

            CommandLogger.Completed(
                "GoForward");

            return;
        }

        CommandLogger.Warning(
            "GoForward",
            "No forward history available");
    }

    public void Refresh()
    {
        CommandLogger.Requested(
            "Refresh");

        if (_browser is null)
        {
            CommandLogger.Warning(
                "Refresh",
                "Browser was null");

            return;
        }

        _browser.Reload();

        CommandLogger.Completed(
            "Refresh");
    }
}
