using Microsoft.UI.Xaml;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using TB.Helpers;
using TB.Infrastructure;
using TB.Models;
using TB.Services.Interfaces;

namespace TB.Services;

public partial class TabManager
{
    public void NextTab() { if (_tabs.Count >= 2) SwitchTab(_tabs[(_tabs.FindIndex(t => t.Id == _activeId) + 1) % _tabs.Count].Id); }
    public void PrevTab() { if (_tabs.Count >= 2) SwitchTab(_tabs[(_tabs.FindIndex(t => t.Id == _activeId) - 1 + _tabs.Count) % _tabs.Count].Id); }
    public void GoToTab(int index) { if (index >= 1 && index <= 9) SwitchToIndex(index); }
    public void GoToLastTab() => SwitchToIndex(9);

    public void MoveTabLeft()
    {
        if (_activeId == -1) return;
        var idx = _tabs.FindIndex(t => t.Id == _activeId);
        if (idx > 0)
        {
            var t = _tabs[idx];
            _tabs.RemoveAt(idx);
            _tabs.Insert(idx - 1, t);
            TabMoved?.Invoke(this, new TabMovedEventArgs { TabId = _activeId, FromIndex = idx, ToIndex = idx - 1 });
            ScheduleSaveSession();
        }
    }

    public void MoveTabRight()
    {
        if (_activeId == -1) return;
        var idx = _tabs.FindIndex(t => t.Id == _activeId);
        if (idx >= 0 && idx < _tabs.Count - 1)
        {
            var t = _tabs[idx];
            _tabs.RemoveAt(idx);
            _tabs.Insert(idx + 1, t);
            TabMoved?.Invoke(this, new TabMovedEventArgs { TabId = _activeId, FromIndex = idx, ToIndex = idx + 1 });
            ScheduleSaveSession();
        }
    }

    public void NavigateActiveTab(string url)
    {
        if (_disposed || _activeId == -1 || !_webViews.TryGetValue(_activeId, out var wv)) return;
        if (_tabs.FirstOrDefault(t => t.Id == _activeId) is not { } tab) return;

        if (UrlResolver.IsInternalUrl(url))
        {
            wv.Source = new Uri(UrlResolver.Resolve(url, _wwwrootPath));
            tab.Url = url;
            tab.Title = UrlResolver.GetTabTitle(url);
            tab.IsInternalPage = true;
            if (_internalPageTabs.Add(_activeId)) SetupInternalPageIpc(wv, _activeId);
        }
        else
        {
            if (_internalPageTabs.Remove(_activeId) && _ipcHandlers.TryGetValue(_activeId, out var prevHandler))
            {
                try { wv.CoreWebView2.WebMessageReceived -= prevHandler; }
                catch (Exception ex) { Logger.Debug($"Prev handler unsubscribe failed: {ex.Message}"); }
                _ipcHandlers.Remove(_activeId);
            }
            wv.Source = new Uri(url);
            tab.Url = url;
            tab.IsInternalPage = false;
        }
        ScheduleSaveSession();
    }

    public void Reload() { if (_activeId != -1 && _webViews.TryGetValue(_activeId, out var wv)) wv.Reload(); }
    public void ReloadTab(int id) { if (!_disposed && _webViews.TryGetValue(id, out var wv)) wv.Reload(); }
    public void Stop() { if (_activeId != -1 && _webViews.TryGetValue(_activeId, out var wv)) wv.CoreWebView2?.Stop(); }
    public void Back() { if (_activeId != -1 && _webViews.TryGetValue(_activeId, out var wv) && wv.CoreWebView2?.CanGoBack == true) wv.GoBack(); }
    public void Forward() { if (_activeId != -1 && _webViews.TryGetValue(_activeId, out var wv) && wv.CoreWebView2?.CanGoForward == true) wv.GoForward(); }

    public async Task SetZoomAsync(int tabId, double zoom)
    {
        if (!_webViews.TryGetValue(tabId, out var wv) || wv.CoreWebView2 == null) return;
        zoom = Math.Clamp(zoom, Defaults.MinZoom, Defaults.MaxZoom);
        _zoomLevels[tabId] = zoom;
        try
        {
            await wv.CoreWebView2.CallDevToolsProtocolMethodAsync("Emulation.setPageScaleFactor", JsonSerializer.Serialize(new { scaleFactor = zoom }));
        }
        catch
        {
            wv.CoreWebView2.PostWebMessageAsJson(JsonSerializer.Serialize(new { c = "zoom", a = new { v = zoom } }));
        }
    }

    public Task ZoomInAsync() => SetZoomAsync(_activeId, GetZoom(_activeId) + Defaults.ZoomStep);
    public Task ZoomOutAsync() => SetZoomAsync(_activeId, GetZoom(_activeId) - Defaults.ZoomStep);
    public Task ResetZoomAsync() => SetZoomAsync(_activeId, Defaults.DefaultZoom);
    private double GetZoom(int id) => _zoomLevels.GetValueOrDefault(id, Defaults.DefaultZoom);

    // FIX: Removed ExecuteScriptAsync to prevent UI jank on rapid keystrokes.
    // Ensure _findBarScript is injected ONCE during CreateTabInternalAsync.
    public Task OpenFindBarAsync()
    {
        if (_activeId != -1 && _webViews.TryGetValue(_activeId, out var wv) && wv.CoreWebView2 != null)
            wv.CoreWebView2.PostWebMessageAsJson(JsonSerializer.Serialize(new { c = "findOpen" }));
        return Task.CompletedTask;
    }

    public Task FindNextAsync()
    {
        if (_activeId != -1 && _webViews.TryGetValue(_activeId, out var wv) && wv.CoreWebView2 != null)
            wv.CoreWebView2.PostWebMessageAsJson(JsonSerializer.Serialize(new { c = "findNext" }));
        return Task.CompletedTask;
    }

    public Task FindPreviousAsync()
    {
        if (_activeId != -1 && _webViews.TryGetValue(_activeId, out var wv) && wv.CoreWebView2 != null)
            wv.CoreWebView2.PostWebMessageAsJson(JsonSerializer.Serialize(new { c = "findPrev" }));
        return Task.CompletedTask;
    }

    public Task CloseFindBarAsync()
    {
        if (_activeId != -1 && _webViews.TryGetValue(_activeId, out var wv) && wv.CoreWebView2 != null)
            wv.CoreWebView2.PostWebMessageAsJson(JsonSerializer.Serialize(new { c = "findClose" }));
        return Task.CompletedTask;
    }

    // FIX: Removed 'async' and return Task.CompletedTask to eliminate CS1998 state machine overhead
    public Task HardReloadAsync()
    {
        if (_activeId != -1 && _webViews.TryGetValue(_activeId, out var wv))
            wv.CoreWebView2?.PostWebMessageAsJson(JsonSerializer.Serialize(new { c = "hardReload" }));
        return Task.CompletedTask;
    }

    public Task PrintAsync()
    {
        if (_activeId != -1 && _webViews.TryGetValue(_activeId, out var wv))
            wv.CoreWebView2?.PostWebMessageAsJson(JsonSerializer.Serialize(new { c = "print" }));
        return Task.CompletedTask;
    }

    public void ViewSource() { if (_activeId != -1 && _webViews.TryGetValue(_activeId, out var wv)) wv.CoreWebView2?.OpenDevToolsWindow(); }
    public void FocusActiveTab() { if (_activeId != -1 && _webViews.TryGetValue(_activeId, out var wv)) wv.Focus(FocusState.Programmatic); }

    public async Task DuplicateTabAsync(int id)
    {
        if (!_disposed) await CreateTabAsync(_tabs.FirstOrDefault(t => t.Id == id)?.Url ?? Defaults.HomeUrl);
    }

    public async Task ReopenLastClosedTabAsync()
    {
        if (_lastClosedUrls.Count > 0)
        {
            var url = _lastClosedUrls[^1];
            _lastClosedUrls.RemoveAt(_lastClosedUrls.Count - 1);
            await CreateTabAsync(url);
        }
    }

    public async Task OpenFeedbackWindowAsync() => await CreateTabAsync(Defaults.FeedbackUrl);
    public void ToggleBookmarksBar() => Logger.Info("Toggle bookmarks bar requested");
    public async Task OpenBookmarksManagerAsync() => await CreateTabAsync(Defaults.BookmarksUrl);
    public async Task OpenHistoryPageAsync() => await CreateTabAsync(Defaults.HistoryUrl);
    public async Task OpenDownloadsPageAsync() => await CreateTabAsync(Defaults.DownloadsUrl);
    public async Task OpenTaskManagerAsync() => await CreateTabAsync(Defaults.TaskManagerUrl);

    public Task OpenDeveloperToolsAsync()
    {
        if (_activeId != -1 && _webViews.TryGetValue(_activeId, out var wv))
            wv.CoreWebView2?.OpenDevToolsWindow();
        return Task.CompletedTask;
    }

    public Task OpenChromeMenuAsync()
    {
        Logger.Info("Open Chrome menu requested");
        return Task.CompletedTask;
    }

    public async Task OpenClearBrowsingDataDialogAsync() => await CreateTabAsync(Defaults.ClearDataUrl);
    public void AddressBarEnd() => FocusActiveTab();

    public Task PageTopAsync()
    {
        if (_activeId != -1 && _webViews.TryGetValue(_activeId, out var wv) && wv.CoreWebView2 != null)
            wv.CoreWebView2.PostWebMessageAsJson(JsonSerializer.Serialize(new { c = "scrollTop" }));
        return Task.CompletedTask;
    }

    public Task PageBottomAsync()
    {
        if (_activeId != -1 && _webViews.TryGetValue(_activeId, out var wv) && wv.CoreWebView2 != null)
            wv.CoreWebView2.PostWebMessageAsJson(JsonSerializer.Serialize(new { c = "scrollBottom" }));
        return Task.CompletedTask;
    }

    public void ToggleFullScreen() => Logger.Info("Toggle full screen requested");
    public void SelectMultipleTabs() => Logger.Info("Select multiple tabs requested");

    public Task CursorWordPreviousAsync()
    {
        if (_activeId != -1 && _webViews.TryGetValue(_activeId, out var wv) && wv.CoreWebView2 != null)
            wv.CoreWebView2.PostWebMessageAsJson(JsonSerializer.Serialize(new { c = "cursorWordPrev" }));
        return Task.CompletedTask;
    }

    public Task CursorWordNextAsync()
    {
        if (_activeId != -1 && _webViews.TryGetValue(_activeId, out var wv) && wv.CoreWebView2 != null)
            wv.CoreWebView2.PostWebMessageAsJson(JsonSerializer.Serialize(new { c = "cursorWordNext" }));
        return Task.CompletedTask;
    }

    public Task DeleteWordPreviousAsync()
    {
        if (_activeId != -1 && _webViews.TryGetValue(_activeId, out var wv) && wv.CoreWebView2 != null)
            wv.CoreWebView2.PostWebMessageAsJson(JsonSerializer.Serialize(new { c = "deleteWordPrev" }));
        return Task.CompletedTask;
    }
}
