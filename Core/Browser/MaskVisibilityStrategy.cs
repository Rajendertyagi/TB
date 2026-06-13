using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace TB.Core.Browser;

public class MaskVisibilityStrategy : IHostVisibilityStrategy
{
    public void Apply(WebViewHost host, bool isActive)
    {
        if (isActive)
        {
            // Bring WebView to front, hide the mask
            Canvas.SetZIndex(host.WebView, 100);
            Canvas.SetZIndex(host.Mask, 0);
            host.Mask.Visibility = Visibility.Collapsed;
        }
        else
        {
            // Push WebView to back, cover it with the opaque mask
            // CRITICAL: WebView.Visibility remains Visible so the GPU keeps rendering it!
            Canvas.SetZIndex(host.WebView, 0);
            Canvas.SetZIndex(host.Mask, 100);
            host.Mask.Visibility = Visibility.Visible;
        }
    }
}
