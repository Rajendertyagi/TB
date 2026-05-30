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

        var webView = new WebView2();

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
        return _webViews.Remove(tabId);
    }
}
