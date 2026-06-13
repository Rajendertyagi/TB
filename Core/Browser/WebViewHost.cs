using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace TB.Core.Browser;

public sealed class WebViewHost : IDisposable
{
    public Guid HostId { get; } = Guid.NewGuid();
    public int TabId { get; }
    public WebView2 WebView { get; }
    public Border Mask { get; }

    private bool _isDisposed;

    public WebViewHost(int tabId, Brush appBackground)
    {
        TabId = tabId;
        WebView = new WebView2
        {
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Visibility = Visibility.Visible // 🛡️ MUST remain visible for GPU rendering
        };
        Mask = new Border
        {
            Background = appBackground,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch,
            Visibility = Visibility.Collapsed
        };
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;
        try { WebView.Close(); } catch { }
    }
}
