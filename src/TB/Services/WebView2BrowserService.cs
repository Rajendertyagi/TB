using Microsoft.Web.WebView2.Wpf;

namespace TB.Services;

public sealed class WebView2BrowserService : IBrowserService
{
    private WebView2? _browser;

    public void Attach(WebView2 browser)
    {
        _browser = browser;
    }

    public void Navigate(string url)
    {
        _browser?.CoreWebView2?.Navigate(url);
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
