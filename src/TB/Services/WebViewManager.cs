using Microsoft.Web.WebView2.Wpf;

namespace TB.Services;

public sealed class WebViewManager : IWebViewManager
{
    private readonly Dictionary<Guid, WebView2> _webViews = [];

    public IReadOnlyCollection<Guid> LoadedTabs =>
        _webViews.Keys.ToList().AsReadOnly();

    public WebView2 Create(Guid tabId)
    {
        if (_webViews.TryGetValue(tabId, out var existing))
        {
            return existing;
        }

        var webView = new WebView2
        {
            HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
            VerticalAlignment = System.Windows.VerticalAlignment.Stretch
        };

        _webViews[tabId] = webView;

        return webView;
    }

    public WebView2? Get(Guid tabId)
    {
        _webViews.TryGetValue(tabId, out var webView);

        return webView;
    }

    public bool Remove(Guid tabId)
    {
        if (_webViews.TryGetValue(tabId, out var webView))
        {
            webView.Dispose();
        }

        return _webViews.Remove(tabId);
    }
}
