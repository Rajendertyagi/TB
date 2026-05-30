using Microsoft.Web.WebView2.Wpf;
using TB.Modules.Logging.Services;

namespace TB.Services;

public sealed class WebView2BrowserService : IBrowserService
{
    private readonly IWebViewManager _webViewManager;

    private WebView2? _browser;

    public WebView2BrowserService(
        IWebViewManager webViewManager)
    {
        _webViewManager = webViewManager;
    }

    public void Attach(WebView2 browser)
    {
        _browser = browser;

        TbLogger.WebViewAttached();
    }

    public void SetActiveBrowser(WebView2 browser)
    {
        _browser = browser;
    }

    public void Navigate(string url)
    {
        TbLogger.NavigationRequested(url);

        try
        {
            if (_browser?.CoreWebView2 is null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(url))
            {
                return;
            }

            if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                url = $"https://{url}";
            }

            _browser.CoreWebView2.Navigate(url);
        }
        catch (Exception ex)
        {
            TbLogger.NavigationFailed(
                url,
                ex);

            throw;
        }
    }

    public void GoBack()
    {
        if (_browser?.CoreWebView2?.CanGoBack == true)
        {
            _browser.CoreWebView2.GoBack();
        }
    }

    public void GoForward()
    {
        if (_browser?.CoreWebView2?.CanGoForward == true)
        {
            _browser.CoreWebView2.GoForward();
        }
    }

    public void Refresh()
    {
        _browser?.Reload();
    }
}
