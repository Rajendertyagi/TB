using Serilog;
using Microsoft.Web.WebView2.Wpf;

namespace TB.Services;

public sealed class WebView2BrowserService : IBrowserService
{
    private WebView2? _browser;

    public void Attach(WebView2 browser)
    {
        _browser = browser;

        Log.Information("WebView attached.");
    }

    public void Navigate(string url)
    {
        Log.Information("Navigate requested: {Url}", url);

        try
        {
            if (_browser?.CoreWebView2 is null)
            {
                Log.Warning("Navigate aborted. WebView2 not initialized.");
                return;
            }

            if (string.IsNullOrWhiteSpace(url))
            {
                Log.Warning("Navigate aborted. URL is empty.");
                return;
            }

            var originalUrl = url;

            if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                url = $"https://{url}";
            }

            Log.Information(
                "URL normalized. Original={OriginalUrl}, Final={FinalUrl}",
                originalUrl,
                url);

            _browser.CoreWebView2.Navigate(url);

            Log.Information("Navigation initiated: {Url}", url);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Navigation failed.");
            throw;
        }
    }

    public void GoBack()
    {
        Log.Information("Back requested.");

        if (_browser?.CoreWebView2?.CanGoBack == true)
        {
            _browser.CoreWebView2.GoBack();
        }
    }

    public void GoForward()
    {
        Log.Information("Forward requested.");

        if (_browser?.CoreWebView2?.CanGoForward == true)
        {
            _browser.CoreWebView2.GoForward();
        }
    }

    public void Refresh()
    {
        Log.Information("Refresh requested.");

        _browser?.Reload();
    }
}