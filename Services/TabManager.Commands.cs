using Microsoft.UI.Xaml;
using System;
using System.Globalization;
using System.Linq;
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
        if (idx > 0) { var t = _tabs[idx]; _tabs.RemoveAt(idx); _tabs.Insert(idx - 1, t); TabMoved?.Invoke(this, new TabMovedEventArgs { TabId = _activeId, FromIndex = idx, ToIndex = idx - 1 }); ScheduleSaveSession(); }
    }

    public void MoveTabRight()
    {
        if (_activeId == -1) return;
        var idx = _tabs.FindIndex(t => t.Id == _activeId);
        if (idx >= 0 && idx < _tabs.Count - 1) { var t = _tabs[idx]; _tabs.RemoveAt(idx); _tabs.Insert(idx + 1, t); TabMoved?.Invoke(this, new TabMovedEventArgs { TabId = _activeId, FromIndex = idx, ToIndex = idx + 1 }); ScheduleSaveSession(); }
    }

    public void NavigateActiveTab(string url)
    {
        if (_disposed || _activeId == -1 || !_webViews.TryGetValue(_activeId, out var wv)) return;
        var tab = _tabs.FirstOrDefault(t => t.Id == _activeId);
        if (tab == null) return;

        if (UrlResolver.IsInternalUrl(url))
        {
            _ = InjectThemeVariablesAsync(wv);
            wv.Source = new Uri(UrlResolver.Resolve(url, _wwwrootPath));
            tab.Url = url; tab.Title = UrlResolver.GetTabTitle(url); tab.IsInternalPage = true;
            if (_internalPageTabs.Add(_activeId)) SetupInternalPageIpc(wv, _activeId);
        }
        else
        {
            if (_internalPageTabs.Remove(_activeId) && _ipcHandlers.TryGetValue(_activeId, out var prevHandler))
            {
                try { wv.CoreWebView2.WebMessageReceived -= prevHandler; } catch { }
                _ipcHandlers.Remove(_activeId);
            }
            wv.Source = new Uri(url); tab.Url = url; tab.IsInternalPage = false;
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
        try { await wv.CoreWebView2.CallDevToolsProtocolMethodAsync("Emulation.setPageScaleFactor", $"{{ \"scaleFactor\": {zoom.ToString(CultureInfo.InvariantCulture)} }}"); }
        catch { await wv.CoreWebView2.ExecuteScriptAsync($"document.body.style.zoom='{zoom}'"); }
    }

    public Task ZoomInAsync() => SetZoomAsync(_activeId, GetZoom(_activeId) + Defaults.ZoomStep);
    public Task ZoomOutAsync() => SetZoomAsync(_activeId, GetZoom(_activeId) - Defaults.ZoomStep);
    public Task ResetZoomAsync() => SetZoomAsync(_activeId, Defaults.DefaultZoom);
    private double GetZoom(int id) => _zoomLevels.GetValueOrDefault(id, Defaults.DefaultZoom);

    public async Task OpenFindBarAsync() { if (_activeId != -1 && _webViews.TryGetValue(_activeId, out var wv) && wv.CoreWebView2 != null) await wv.CoreWebView2.ExecuteScriptAsync(_findBarScript.Value); }
    public async Task FindNextAsync() { if (_activeId != -1 && _webViews.TryGetValue(_activeId, out var wv) && wv.CoreWebView2 != null) { await wv.CoreWebView2.ExecuteScriptAsync(_findBarScript.Value); await wv.CoreWebView2.ExecuteScriptAsync("window.__findNext && window.__findNext();"); } }
    public async Task FindPreviousAsync() { if (_activeId != -1 && _webViews.TryGetValue(_activeId, out var wv) && wv.CoreWebView2 != null) { await wv.CoreWebView2.ExecuteScriptAsync(_findBarScript.Value); await wv.CoreWebView2.ExecuteScriptAsync("window.__findPrev && window.__findPrev();"); } }
    public async Task CloseFindBarAsync() { if (_activeId != -1 && _webViews.TryGetValue(_activeId, out var wv) && wv.CoreWebView2 != null) await wv.CoreWebView2.ExecuteScriptAsync("window.__closeFindBar && window.__closeFindBar();"); }

    public async Task HardReloadAsync() { if (_activeId != -1 && _webViews.TryGetValue(_activeId, out var wv)) await wv.CoreWebView2.ExecuteScriptAsync("location.reload(true);"); }
    public async Task PrintAsync() { if (_activeId != -1 && _webViews.TryGetValue(_activeId, out var wv)) await wv.CoreWebView2.ExecuteScriptAsync("window.print();"); }
    public void ViewSource() { if (_activeId != -1 && _webViews.TryGetValue(_activeId, out var wv)) wv.CoreWebView2?.OpenDevToolsWindow(); }
    public void FocusActiveTab() { if (_activeId != -1 && _webViews.TryGetValue(_activeId, out var wv)) wv.Focus(FocusState.Programmatic); }
    public async Task DuplicateTabAsync(int id) { if (!_disposed) await CreateTabAsync(_tabs.FirstOrDefault(t => t.Id == id)?.Url ?? Defaults.HomeUrl); }
    public async Task ReopenLastClosedTabAsync() { if (_lastClosedUrls.Count > 0) { var url = _lastClosedUrls[^1]; _lastClosedUrls.RemoveAt(_lastClosedUrls.Count - 1); await CreateTabAsync(url); } }

    public async Task OpenFeedbackWindowAsync() => await CreateTabAsync(Defaults.FeedbackUrl);
    public void ToggleBookmarksBar() => Logger.Info("Toggle bookmarks bar requested");
    public async Task OpenBookmarksManagerAsync() => await CreateTabAsync(Defaults.BookmarksUrl);
    public async Task OpenHistoryPageAsync() => await CreateTabAsync(Defaults.HistoryUrl);
    public async Task OpenDownloadsPageAsync() => await CreateTabAsync(Defaults.DownloadsUrl);
    public async Task OpenTaskManagerAsync() => await CreateTabAsync(Defaults.TaskManagerUrl);
    public async Task OpenDeveloperToolsAsync() { if (_activeId != -1 && _webViews.TryGetValue(_activeId, out var wv)) wv.CoreWebView2?.OpenDevToolsWindow(); }
    public async Task OpenChromeMenuAsync() => Logger.Info("Open Chrome menu requested");
    public async Task OpenClearBrowsingDataDialogAsync() => await CreateTabAsync(Defaults.ClearDataUrl);
    public void AddressBarEnd() => FocusActiveTab();

    public Task PageTopAsync()
    {
        if (_activeId != -1 && _webViews.TryGetValue(_activeId, out var wv) && wv.CoreWebView2 != null)
            return wv.CoreWebView2.ExecuteScriptAsync("window.scrollTo(0, 0);").AsTask();
        return Task.CompletedTask;
    }

    public Task PageBottomAsync()
    {
        if (_activeId != -1 && _webViews.TryGetValue(_activeId, out var wv) && wv.CoreWebView2 != null)
            return wv.CoreWebView2.ExecuteScriptAsync("window.scrollTo(0, document.body.scrollHeight);").AsTask();
        return Task.CompletedTask;
    }

    public void ToggleFullScreen() => Logger.Info("Toggle full screen requested");
    public void SelectMultipleTabs() => Logger.Info("Select multiple tabs requested");

    // FIX: Added missing interface implementations to resolve CS0535 errors
    public Task CursorWordPreviousAsync()
    {
        if (_activeId != -1 && _webViews.TryGetValue(_activeId, out var wv) && wv.CoreWebView2 != null)
            return wv.CoreWebView2.ExecuteScriptAsync("var selection = window.getSelection(); var range = selection.getRangeAt(0); var newRange = document.createRange(); var node = range.startContainer; while (node.previousSibling) { newRange.setStart(node.previousSibling, 0); break; } selection.removeAllRanges(); selection.addRange(newRange);").AsTask();
        return Task.CompletedTask;
    }

    public Task CursorWordNextAsync()
    {
        if (_activeId != -1 && _webViews.TryGetValue(_activeId, out var wv) && wv.CoreWebView2 != null)
            return wv.CoreWebView2.ExecuteScriptAsync("var selection = window.getSelection(); var range = selection.getRangeAt(0); var newRange = document.createRange(); var node = range.startContainer; while (node.nextSibling) { newRange.setStart(node.nextSibling, 0); break; } selection.removeAllRanges(); selection.addRange(newRange);").AsTask();
        return Task.CompletedTask;
    }

    public Task DeleteWordPreviousAsync()
    {
        if (_activeId != -1 && _webViews.TryGetValue(_activeId, out var wv) && wv.CoreWebView2 != null)
            return wv.CoreWebView2.ExecuteScriptAsync("var selection = window.getSelection(); var range = selection.getRangeAt(0); var node = range.startContainer; if (node.nodeType === Node.TEXT_NODE) { var text = node.textContent; var before = text.substring(0, range.startOffset - 1); var after = text.substring(range.startOffset); node.textContent = before + after; range.setStart(node, before.length); selection.removeAllRanges(); selection.addRange(range); }").AsTask();
        return Task.CompletedTask;
    }
}
