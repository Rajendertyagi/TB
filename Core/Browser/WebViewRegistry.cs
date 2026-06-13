using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TB.Core.Browser;

public sealed class WebViewRegistry
{
    private readonly Dictionary<Guid, WebViewHost> _hostsByHostId = new();
    private readonly Dictionary<int, Guid> _hostIdByTabId = new();
    private readonly Grid _surface;
    private readonly CoreWebView2Environment _env;
    private readonly IHostVisibilityStrategy _visibilityStrategy;
    private readonly Brush _appBackground;

    public WebViewRegistry(Grid surface, CoreWebView2Environment env, IHostVisibilityStrategy strategy, Brush appBackground)
    {
        _surface = surface;
        _env = env;
        _visibilityStrategy = strategy;
        _appBackground = appBackground;
    }

    public async Task<WebViewHost> CreateHostAsync(int tabId)
    {
        if (_hostIdByTabId.TryGetValue(tabId, out var existingId))
            return _hostsByHostId[existingId];

        var host = new WebViewHost(tabId, _appBackground);

        // 1. Attach to Visual Tree FIRST (Required for HWND allocation)
        _surface.Children.Add(host.WebView);
        _surface.Children.Add(host.Mask);

        // 2. Industry Standard: Prevent White Flash
        // Must be set BEFORE EnsureCoreWebView2Async
        host.WebView.DefaultBackgroundColor = Colors.Transparent;

        // 3. Industry Standard: Force Layout Pass
        // Guarantees the control has ActualWidth/Height > 0 before Chromium initializes.
        // This prevents the "0x0 viewport" race condition where navigation succeeds 
        // but the renderer has no pixels to paint into (blank page).
        host.WebView.UpdateLayout();

        // 4. Initialize CoreWebView2
        await host.WebView.EnsureCoreWebView2Async(_env);

        _hostsByHostId[host.HostId] = host;
        _hostIdByTabId[tabId] = host.HostId;

        // Hide until activated
        _visibilityStrategy.Apply(host, false);

        return host;
    }

    public void ActivateTab(int tabId)
    {
        if (!_hostIdByTabId.TryGetValue(tabId, out var targetHostId)) return;

        foreach (var kvp in _hostsByHostId)
        {
            _visibilityStrategy.Apply(kvp.Value, kvp.Key == targetHostId);
        }

        if (_hostsByHostId.TryGetValue(targetHostId, out var activeHost))
        {
            activeHost.WebView.Focus(FocusState.Programmatic);
        }
    }

    public void DestroyTab(int tabId)
    {
        if (!_hostIdByTabId.TryGetValue(tabId, out var hostId)) return;
        if (!_hostsByHostId.TryGetValue(hostId, out var host)) return;

        _surface.Children.Remove(host.WebView);
        _surface.Children.Remove(host.Mask);
        host.Dispose();

        _hostsByHostId.Remove(hostId);
        _hostIdByTabId.Remove(tabId);
    }

    public WebView2? GetWebView(int tabId)
    {
        if (_hostIdByTabId.TryGetValue(tabId, out var hostId) && _hostsByHostId.TryGetValue(hostId, out var host))
            return host.WebView;
        return null;
    }

    public void ClearAll()
    {
        foreach (var host in _hostsByHostId.Values)
        {
            _surface.Children.Remove(host.WebView);
            _surface.Children.Remove(host.Mask);
            host.Dispose();
        }
        _hostsByHostId.Clear();
        _hostIdByTabId.Clear();
    }
}