using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using TB.Features.Downloads;
using TB.Helpers;
using TB.Infrastructure;
using Windows.Foundation;

namespace TB.Services;

public static class IpcActions
{
    public const string DownloadProgress = "DOWNLOAD_PROGRESS";
    public const string DownloadCompleted = "DOWNLOAD_COMPLETED";
    public const string DownloadFailed = "DOWNLOAD_FAILED";
    public const string SettingsReady = "SETTINGS_READY";
    public const string SaveSetting = "SAVE_SETTING";
    public const string GetDownloads = "GET_DOWNLOADS";
    public const string RemoveDownload = "REMOVE_DOWNLOAD";
    public const string ClearDownloads = "CLEAR_DOWNLOADS";
    public const string CloseSettings = "CLOSE_SETTINGS";
    public const string BrowseFolder = "BROWSE_FOLDER";
    public const string ThemeUpdate = "THEME_UPDATE";
    public const string SettingsData = "SETTINGS_DATA";
    public const string DownloadsList = "DOWNLOADS_LIST";
}

public partial class TabManager
{
    internal void SetupInternalPageIpc(WebView2 wv, int tabId)
    {
        TypedEventHandler<CoreWebView2, CoreWebView2WebMessageReceivedEventArgs> handler = async (s, e) =>
        {
            if (!_internalPageTabs.Contains(tabId)) return;
            if (!(e.Source ?? "").StartsWith("tb://", StringComparison.OrdinalIgnoreCase)) return;

            try
            {
                using var doc = JsonDocument.Parse(e.WebMessageAsJson);
                if (!doc.RootElement.TryGetProperty("action", out var actionProp)) return;
                await RouteIpcMessage(tabId, actionProp.GetString() ?? "", doc.RootElement, wv);
            }
            catch (Exception ex) { Logger.Error($"IPC error: {ex.Message}"); }
        };

        wv.CoreWebView2.WebMessageReceived += handler;
        _ipcHandlers[tabId] = handler;
    }

    private async Task RouteIpcMessage(int tabId, string action, JsonElement root, WebView2 wv)
    {
        switch (action)
        {
            case IpcActions.SettingsReady:
                var settingsJson = _settingsService.GetAllJson();
                await wv.CoreWebView2.ExecuteScriptAsync($"window.dispatchEvent(new MessageEvent('message', {{ data: {{ action: '{IpcActions.SettingsData}', settings: {settingsJson} }} }}))");
                break;
            case IpcActions.SaveSetting:
                if (root.TryGetProperty("key", out var k) && root.TryGetProperty("value", out var v))
                {
                    var key = k.GetString() ?? "";
                    if (v.ValueKind == JsonValueKind.True || v.ValueKind == JsonValueKind.False) _settingsService.Set(key, v.GetBoolean());
                    else if (v.ValueKind == JsonValueKind.Number) _settingsService.Set(key, v.GetInt32());
                    else if (v.ValueKind == JsonValueKind.String) _settingsService.Set(key, v.GetString() ?? "");
                }
                break;
            case IpcActions.GetDownloads:
                var list = _downloads.Downloads.Select(DownloadViewModel.FromItem).ToList();
                var json = JsonSerializer.Serialize(new { action = IpcActions.DownloadsList, downloads = list });
                await wv.CoreWebView2.ExecuteScriptAsync($"window.dispatchEvent(new MessageEvent('message', {{ data: {json} }}))");
                break;
            case IpcActions.RemoveDownload:
                if (root.TryGetProperty("id", out var rmId)) { _downloadOwners.Remove(rmId.GetInt32()); _downloads.RemoveDownload(rmId.GetInt32()); }
                break;
            case IpcActions.ClearDownloads: _downloadOwners.Clear(); _downloads.ClearAll(); break;
            case IpcActions.CloseSettings: CloseTab(tabId); break;
        }
    }

    private void HandleDownloadStarting(object? sender, CoreWebView2DownloadStartingEventArgs args, int tabId)
    {
        if (tabId == 0) return;
        try
        {
            var defaultPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
            var downloadDir = _settingsService.Get<string>("download-path", defaultPath) ?? defaultPath;
            Directory.CreateDirectory(downloadDir);

            var fileName = Path.GetFileName(args.ResultFilePath);
            if (string.IsNullOrEmpty(fileName)) fileName = "download_" + DateTime.Now.Ticks;

            var proposedPath = Path.GetFullPath(Path.Combine(downloadDir, fileName));
            if (!proposedPath.StartsWith(Path.GetFullPath(downloadDir), StringComparison.OrdinalIgnoreCase))
            {
                fileName = "download_" + DateTime.Now.Ticks;
                proposedPath = Path.GetFullPath(Path.Combine(downloadDir, fileName));
            }

            args.ResultFilePath = proposedPath;
            var dlItem = _downloads.AddDownload(args.DownloadOperation.Uri, fileName, proposedPath, (long)args.DownloadOperation.TotalBytesToReceive);
            _downloadOwners[dlItem.Id] = tabId;
            args.Handled = true;

            var op = args.DownloadOperation;
            op.StateChanged += (_, _) =>
            {
                if (op.State == CoreWebView2DownloadState.Completed) _downloads.CompleteDownload(dlItem.Id);
                else if (op.State == CoreWebView2DownloadState.Interrupted) _downloads.FailDownload(dlItem.Id, "Interrupted");
            };
            op.BytesReceivedChanged += (_, _) => _downloads.UpdateProgress(dlItem.Id, (long)op.BytesReceived);
        }
        catch (Exception ex) { Logger.Error($"Download error: {ex.Message}"); }
    }

    private void OnDownloadProgress(DownloadItem item) => SendToDownloadOwner(item.Id, new { action = IpcActions.DownloadProgress, id = item.Id, name = item.Name, url = item.Url, progressPercent = item.ProgressPercent, totalBytes = item.TotalBytes, status = item.Status });
    private void OnDownloadCompleted(DownloadItem item) => SendToDownloadOwner(item.Id, new { action = IpcActions.DownloadCompleted, id = item.Id, filePath = item.FilePath });
    private void OnDownloadFailed(DownloadItem item) => SendToDownloadOwner(item.Id, new { action = IpcActions.DownloadFailed, id = item.Id, error = "Download failed" });

    private void SendToDownloadOwner(int downloadId, object data)
    {
        Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread()?.TryEnqueue(() =>
        {
            var json = JsonSerializer.Serialize(data);
            var targetIds = new HashSet<int>();
            if (_downloadOwners.TryGetValue(downloadId, out var ownerId)) targetIds.Add(ownerId);
            foreach (var id in _internalPageTabs) { if (_tabs.Any(t => t.Id == id && t.Url == "tb://downloads")) targetIds.Add(id); }

            foreach (var id in targetIds)
            {
                if (_webViews.TryGetValue(id, out var wv))
                {
                    try { wv.CoreWebView2?.PostWebMessageAsJson(json); } catch { }
                }
            }
        });
    }

    private void OnThemeChanged()
    {
        Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread()?.TryEnqueue(async () =>
        {
            foreach (var id in _internalPageTabs)
            {
                if (_webViews.TryGetValue(id, out var wv)) await InjectThemeVariablesAsync(wv);
            }
        });
    }

    internal async Task InjectThemeVariablesAsync(WebView2 wv)
    {
        if (wv.CoreWebView2 == null) return;
        var themeVars = _themeService.GetCssVariables();
        var varsJson = JsonSerializer.Serialize(themeVars);
        await wv.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync($"window.__themeVariables = {varsJson};");
    }
}

