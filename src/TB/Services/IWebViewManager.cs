using Microsoft.Web.WebView2.Wpf;

namespace TB.Services;

public interface IWebViewManager
{
    WebView2 Create(Guid tabId);

    WebView2? Get(Guid tabId);

    bool Remove(Guid tabId);

    IReadOnlyCollection<Guid> LoadedTabs { get; }

    int LoadedTabCount { get; }
}
