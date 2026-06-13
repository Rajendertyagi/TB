using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using TB.Helpers;
using TB.Infrastructure;
using TB.Input;
using TB.Models;
using TB.Services.Interfaces;

namespace TB.Services;

public partial class TabManager
{
    public Task CreateTabAsync(string url = Defaults.HomeUrl) => CreateTabInternalAsync(url, false);

    internal async Task CreateTabInternalAsync(string url, bool deferNavigation)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        EnsureInitialized();

        int id = _nextId++;

        
        var host = await _registry!.CreateHostAsync(id);
        var webView = host.WebView;

        try
        {
            if (webView.CoreWebView2 == null)
            {
                Logger.Error($"CoreWebView2 is null for tab {id} after EnsureCoreWebView2Async. WebView2 runtime might be locked or uninstalled.");
                return;
            }

            if (!string.IsNullOrEmpty(_bridgeScript.Value))
                await webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(_bridgeScript.Value);

            var initThemeVars = _themeService.GetCssVariables();
            var initThemeJson = JsonSerializer.Serialize(initThemeVars);
            await webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync($"window.__themeVariables = {initThemeJson};");

            if (!string.IsNullOrEmpty(_themeSyncScript.Value))
                await webView.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(_themeSyncScript.Value);

            try { webView.CoreWebView2.Settings.IsZoomControlEnabled = false; } catch (Exception ex) { Logger.Debug("Disable zoom control failed", ex); }
            try { webView.CoreWebView2.Settings.AreDefaultContextMenusEnabled = true; } catch (Exception ex) { Logger.Debug("Enable context menus failed", ex); }

            var handlers = CreateEventHandlers(id, webView);
            AttachCoreEvents(webView.CoreWebView2, handlers, id);
            _wvHandlers[id] = handlers;

            // Replaced by global WH_KEYBOARD_LL hook in KeyboardShortcutHandler.cs

            webView.KeyDown += (_, e) =>
            {
                if (GetWebView(id) == null) return;
                if (_keyboardHandler.HandleKey((Windows.System.VirtualKey)e.Key)) e.Handled = true;
            };

            var tabItem = new TabItem { Id = id, Url = url, Title = UrlResolver.GetTabTitle(url), Zoom = Defaults.DefaultZoom };

            if (!deferNavigation)
            {
                if (UrlResolver.IsInternalUrl(url))
                {
                    tabItem.IsInternalPage = true;
                    webView.Source = new Uri(UrlResolver.Resolve(url, _wwwrootPath));
                    SetupInternalPageIpc(webView, id);
                    _internalPageTabs.Add(id);
                }
                else
                {
                    webView.Source = new Uri(url);
                }
            }
            else if (UrlResolver.IsInternalUrl(url))
            {
                tabItem.IsInternalPage = true;
                _internalPageTabs.Add(id);
            }

            _tabs.Add(tabItem);
            _zoomLevels[id] = Defaults.DefaultZoom;

            TabCreated?.Invoke(this, new TabEventArgs { Id = id, Title = tabItem.Title, Url = url });
            SwitchTab(id);
            ScheduleSaveSession();
        }
        catch
        {
            
            _registry?.DestroyTab(id);
            throw;
        }
    }

    public void SwitchTab(int id)
    {
        if (_disposed || GetWebView(id) == null) return;

        _registry?.ActivateTab(id);
        _activeId = id;

        var wv = GetWebView(id);
        if (wv == null) return;

        var tab = _tabs.FirstOrDefault(t => t.Id == id);

        // FIX: Use OriginalString for safer URI comparison
        if (wv.Source == null || wv.Source.OriginalString == "about:blank")
        {
            var url = tab?.Url ?? Defaults.HomeUrl;
            if (UrlResolver.IsInternalUrl(url))
            {
                wv.Source = new Uri(UrlResolver.Resolve(url, _wwwrootPath));
                if (!_ipcHandlers.ContainsKey(id)) SetupInternalPageIpc(wv, id);
            }
            else
            {
                wv.Source = new Uri(url);
            }
        }

        TabSwitched?.Invoke(this, new TabEventArgs { Id = id, Title = tab?.Title ?? "", Url = tab?.Url ?? "" });

        try
        {
            UrlChanged?.Invoke(this, new UrlEventArgs { Url = wv.Source?.OriginalString ?? "" });
        }
        catch (Exception ex)
        {
            Logger.Warning($"UrlChanged notification failed: {ex.Message}");
        }

        FireNavState(id);
        ScheduleSaveSession();
    }

    public void SwitchToIndex(int index)
    {
        if (_tabs.Count > 0)
            SwitchTab(_tabs[index == 9 ? _tabs.Count - 1 : Math.Clamp(index - 1, 0, _tabs.Count - 1)].Id);
    }

    public void CloseTab(int id)
    {
        if (_disposed) return;
        var wv = GetWebView(id);
        if (wv == null) return;

        var closedTab = _tabs.FirstOrDefault(t => t.Id == id);

        if (closedTab is not null && !string.IsNullOrEmpty(closedTab.Url))
        {
            _lastClosedUrls.Add(closedTab.Url);
            if (_lastClosedUrls.Count > 10) _lastClosedUrls.RemoveAt(0);
        }

        var closedIdx = _tabs.FindIndex(t => t.Id == id);

        DetachAndCleanState(id, wv);
        ScheduleSaveSession();
        TabClosed?.Invoke(this, new TabEventArgs { Id = id });

        if (_activeId == id && _tabs.Count > 0)
        {
            var nextIdx = Math.Min(closedIdx, _tabs.Count - 1);
            SwitchTab(_tabs[Math.Max(0, nextIdx)].Id);
        }
        else if (_tabs.Count == 0)
        {
            TabsCleared?.Invoke(this, EventArgs.Empty);
            BeforeShutdown?.Invoke(this, EventArgs.Empty);
            if (_dispatcherQueue.HasThreadAccess) Application.Current.Exit();
            else _dispatcherQueue.TryEnqueue(Application.Current.Exit);
        }


    }

    internal void DetachAndCleanState(int id, WebView2 wv)
    {
        if (_ipcHandlers.TryGetValue(id, out var ipcHandler))
        {
            try { wv.CoreWebView2.WebMessageReceived -= ipcHandler; } catch (Exception ex) { Logger.Debug("WebMessageReceived unsubscribe failed", ex); }
            _ipcHandlers.Remove(id);
        }

        if (_wvHandlers.TryGetValue(id, out var handlers))
        {
            var core = wv.CoreWebView2;
            if (core != null)
            {
                if (handlers.ProcessFailed != null) core.ProcessFailed -= handlers.ProcessFailed;
                if (handlers.NewWindowRequested != null) core.NewWindowRequested -= handlers.NewWindowRequested;
                if (handlers.NavigationStarting != null) core.NavigationStarting -= handlers.NavigationStarting;
                if (handlers.NavigationCompleted != null) core.NavigationCompleted -= handlers.NavigationCompleted;
                if (handlers.DocumentTitleChanged != null) core.DocumentTitleChanged -= handlers.DocumentTitleChanged;
                if (handlers.SourceChanged != null) core.SourceChanged -= handlers.SourceChanged;
                if (handlers.FaviconChanged != null) core.FaviconChanged -= handlers.FaviconChanged;
                if (handlers.ContextMenuRequested != null) core.ContextMenuRequested -= handlers.ContextMenuRequested;
            }
            _wvHandlers.Remove(id);
        }

        if (_acceleratorSubscriptions.TryGetValue(id, out var accelSub))
        {
            accelSub.Dispose();
            _acceleratorSubscriptions.Remove(id);
        }

        _registry?.DestroyTab(id);

        _zoomLevels.Remove(id);
        _internalPageTabs.Remove(id);
        _lastFaviconTimestamp.Remove(id);

        foreach (var key in _downloadOwners.Where(kv => kv.Value == id).Select(kv => kv.Key).ToList())
            _downloadOwners.Remove(key);

        _tabs.RemoveAll(t => t.Id == id);
    }

    public void CloseActiveTab() => CloseTab(_activeId);

    public void CloseOtherTabs(int id)
    {
        foreach (var tab in _tabs.Where(t => t.Id != id).ToList()) CloseTab(tab.Id);
    }

    public void CloseTabsToTheRight(int id)
    {
        var idx = _tabs.FindIndex(t => t.Id == id);
        if (idx >= 0) foreach (var tab in _tabs.Skip(idx + 1).ToList()) CloseTab(tab.Id);
    }
}