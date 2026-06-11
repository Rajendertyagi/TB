using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using TB.Helpers;
using TB.Infrastructure;
using TB.Models;
using TB.Services.Interfaces;
using Windows.Foundation;

namespace TB.Services;

public partial class TabManager
{
    internal class WebView2EventHandlers
    {
        public TypedEventHandler<CoreWebView2, CoreWebView2ProcessFailedEventArgs>? ProcessFailed;
        public TypedEventHandler<CoreWebView2, CoreWebView2NewWindowRequestedEventArgs>? NewWindowRequested;
        public TypedEventHandler<CoreWebView2, CoreWebView2NavigationStartingEventArgs>? NavigationStarting;
        public TypedEventHandler<CoreWebView2, CoreWebView2NavigationCompletedEventArgs>? NavigationCompleted;
        public TypedEventHandler<CoreWebView2, object>? DocumentTitleChanged;
        public TypedEventHandler<CoreWebView2, CoreWebView2SourceChangedEventArgs>? SourceChanged;
        public TypedEventHandler<CoreWebView2, object>? FaviconChanged;
        public TypedEventHandler<CoreWebView2, CoreWebView2ContextMenuRequestedEventArgs>? ContextMenuRequested;
    }

    internal WebView2EventHandlers CreateEventHandlers(int id, WebView2 webView)
    {
        return new WebView2EventHandlers
        {
            ProcessFailed = (s, args) => HandleProcessFailed(id, args),
            NewWindowRequested = (s, args) => HandleNewWindowRequested(id, args),
            NavigationStarting = (s, args) => HandleNavigationStarting(id, args),
            NavigationCompleted = (s, args) => HandleNavigationCompleted(id),
            DocumentTitleChanged = (s, args) => HandleDocumentTitleChanged(id, webView),
            SourceChanged = (s, args) => HandleSourceChanged(id, webView),
            FaviconChanged = async (s, args) => await HandleFaviconChanged(id, webView),
            ContextMenuRequested = (s, args) => HandleContextMenuRequested(id, args)
        };
    }

    internal void AttachCoreEvents(CoreWebView2 core, WebView2EventHandlers handlers, int tabId)
    {
        core.ProcessFailed += handlers.ProcessFailed;
        core.NewWindowRequested += handlers.NewWindowRequested;
        core.NavigationStarting += handlers.NavigationStarting;
        core.NavigationCompleted += handlers.NavigationCompleted;
        core.DocumentTitleChanged += handlers.DocumentTitleChanged;
        core.SourceChanged += handlers.SourceChanged;
        core.FaviconChanged += handlers.FaviconChanged;
        core.ContextMenuRequested += handlers.ContextMenuRequested;
        core.DownloadStarting += (s, args) => HandleDownloadStarting(s, args, tabId);
    }

    private void HandleProcessFailed(int id, CoreWebView2ProcessFailedEventArgs args)
    {
        if (!_webViews.ContainsKey(id)) return;
        Logger.Error($"WebView process failed for tab {id}: {args.Reason}");
        if (_webViews.TryGetValue(id, out var wv))
        {
            try
            {
                var crashUrl = _tabs.FirstOrDefault(t => t.Id == id)?.Url ?? "";
                var safeUrl = JsonSerializer.Serialize(crashUrl);
                var html = $@"<html><body style='background:#13141a;color:#c8cdd8;display:flex;align-items:center;justify-content:center;font-family:sans-serif;'><h2>Crashed</h2><p onclick='window.location={safeUrl}'>Reload</p></body></html>";
                wv.CoreWebView2?.NavigateToString(html);
            }
            catch (Exception ex) { Logger.Error($"Crash page render failed: {ex.Message}"); }
        }
    }

    private void HandleNewWindowRequested(int id, CoreWebView2NewWindowRequestedEventArgs args)
    {
        if (!_webViews.ContainsKey(id)) return;
        args.Handled = true;
        if (!string.IsNullOrEmpty(args.Uri)) _ = CreateTabInternalAsync(args.Uri, false);
    }

    private void HandleNavigationStarting(int id, CoreWebView2NavigationStartingEventArgs args)
    {
        if (!_webViews.ContainsKey(id)) return;
        if (id == _activeId)
        {
            UrlChanged?.Invoke(this, new UrlEventArgs { Url = args.Uri });
            NavigationStarted?.Invoke(this, EventArgs.Empty);
        }
    }

    private void HandleNavigationCompleted(int id)
    {
        if (!_webViews.ContainsKey(id)) return;
        if (id == _activeId && !_disposed)
        {
            var tab = _tabs.FirstOrDefault(t => t.Id == id);
            if (tab != null && _webViews.TryGetValue(id, out var wv)) tab.Title = wv.CoreWebView2?.DocumentTitle ?? tab.Title;
            NavigationCompleted?.Invoke(this, EventArgs.Empty);
        }
        FireNavState(id);
    }

    private void HandleDocumentTitleChanged(int id, WebView2 webView)
    {
        if (!_webViews.ContainsKey(id)) return;
        var tab = _tabs.FirstOrDefault(t => t.Id == id);
        if (tab != null && !tab.IsInternalPage)
        {
            tab.Title = webView.CoreWebView2?.DocumentTitle ?? tab.Title;
            tab.Url = webView.CoreWebView2?.Source?.ToString() ?? tab.Url;
            ScheduleSaveSession();
        }
    }

    private void HandleSourceChanged(int id, WebView2 webView)
    {
        if (!_webViews.ContainsKey(id)) return;
        var src = webView.CoreWebView2?.Source?.ToString() ?? "";
        if (id == _activeId && !string.IsNullOrEmpty(src)) UrlChanged?.Invoke(this, new UrlEventArgs { Url = src });
        var srcTab = _tabs.FirstOrDefault(t => t.Id == id);
        if (srcTab != null && !string.IsNullOrEmpty(src) && src != srcTab.Url)
        {
            srcTab.Url = src;
            ScheduleSaveSession();
        }
    }

    private async Task HandleFaviconChanged(int id, WebView2 webView)
    {
        if (!_webViews.ContainsKey(id)) return;
        var now = Stopwatch.GetTimestamp();
        if (now - _lastFaviconTimestamp.GetValueOrDefault(id) < Stopwatch.Frequency) return;
        _lastFaviconTimestamp[id] = now;
        try
        {
            using var stream = await webView.CoreWebView2.GetFaviconAsync(CoreWebView2FaviconImageFormat.Png);
            if (stream == null) return;
            var bitmap = new Microsoft.UI.Xaml.Media.Imaging.BitmapImage();
            await bitmap.SetSourceAsync(stream);
            FaviconUpdated?.Invoke(this, new FaviconEventArgs { TabId = id, Favicon = bitmap });
        }
        catch (Exception ex) { Logger.Warning($"Failed loading favicon: {ex.Message}"); }
    }

    private void HandleContextMenuRequested(int id, CoreWebView2ContextMenuRequestedEventArgs args)
    {
        if (!_webViews.ContainsKey(id)) return;
        var deferral = args.GetDeferral();
        try { /* Custom menu logic here */ } finally { deferral.Complete(); }
    }

    internal void FireNavState(int id)
    {
        if (_disposed || !_webViews.TryGetValue(id, out var wv)) return;
        try
        {
            var core = wv.CoreWebView2;
            NavStateChanged?.Invoke(this, new NavStateEventArgs { CanGoBack = core?.CanGoBack ?? false, CanGoForward = core?.CanGoForward ?? false, Title = _tabs.FirstOrDefault(t => t.Id == id)?.Title ?? "" });
        }
        catch { }
    }
}

