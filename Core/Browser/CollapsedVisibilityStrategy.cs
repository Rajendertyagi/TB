using Microsoft.UI.Xaml;

namespace TB.Core.Browser;

public class CollapsedVisibilityStrategy : IHostVisibilityStrategy
{
    public void Apply(WebViewHost host, bool isActive)
    {
        host.WebView.Visibility = isActive ? Visibility.Visible : Visibility.Collapsed;
    }
}