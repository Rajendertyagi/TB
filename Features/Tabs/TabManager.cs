using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using TB.Infrastructure;

namespace TB.Features.Tabs
{
    public class TabManager
    {
        private readonly Grid _contentGrid;
        private readonly CoreWebView2 _shellCore;
        private readonly Dictionary<int, WebView2> _tabs = new();
        private int _nextId = 1;
        private int _activeId = -1;
        private double _zoomLevel = 1.0;

        public TabManager(Grid contentGrid, CoreWebView2 shellCore)
        {
            _contentGrid = contentGrid;
            _shellCore = shellCore;
        }

        public async Task CreateTabAsync(string url = "https://www.google.com")
        {
            int id = _nextId++;
            var webView = new WebView2 { HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch };
            _contentGrid.Children.Add(webView);
            await webView.EnsureCoreWebView2Async(_shellCore.Environment);
            webView.Visibility = Visibility.Collapsed;

            webView.CoreWebView2.NavigationStarting += (s, args) => { if (id == _activeId) SendToJs(new { action = "UPDATE_URL", url = args.Uri }); };
            webView.Source = new Uri(url);
            _tabs[id] = webView;
            SendToJs(new { action = "TAB_CREATED", id, title = "New Tab" });
            SwitchTab(id);
        }

        public void SwitchTab(int id)
        {
            if (!_tabs.ContainsKey(id)) return;
            foreach (var tab in _tabs.Values) tab.Visibility = Visibility.Collapsed;
            _tabs[id].Visibility = Visibility.Visible; _activeId = id;
            SendToJs(new { action = "TAB_SWITCHED", id });
            SendToJs(new { action = "UPDATE_URL", url = _tabs[id].Source?.ToString() ?? "" });
        }

        public void SwitchToIndex(int index)
        {
            if (_tabs.Count == 0) return;
            var keys = _tabs.Keys.ToList();
            int target = (index == 9) ? keys.Count - 1 : index - 1;
            if (target >= 0 && target < keys.Count) SwitchTab(keys[target]);
        }

        public void CloseTab(int id)
        {
            if (!_tabs.ContainsKey(id)) return;
            _contentGrid.Children.Remove(_tabs[id]); _tabs.Remove(id);
            SendToJs(new { action = "TAB_CLOSED", id });
            if (_activeId == id && _tabs.Count > 0) SwitchTab(_tabs.Keys.Last());
            else if (_tabs.Count == 0) Application.Current.Exit();
        }

        public void NavigateActiveTab(string url) { if (_activeId != -1 && _tabs.ContainsKey(_activeId)) _tabs[_activeId].Source = new Uri(url); }
        public void CloseActiveTab() => CloseTab(_activeId);
        public void NextTab() { if (_tabs.Count == 0) return; var keys = _tabs.Keys.ToList(); SwitchTab(keys[(keys.IndexOf(_activeId) + 1) % keys.Count]); }
        public void PrevTab() { if (_tabs.Count == 0) return; var keys = _tabs.Keys.ToList(); SwitchTab(keys[(keys.IndexOf(_activeId) - 1 + keys.Count) % keys.Count]); }

        public void Reload() { if (_activeId != -1 && _tabs.ContainsKey(_activeId)) _tabs[_activeId].Reload(); }
        public void Stop() { if (_activeId != -1 && _tabs.ContainsKey(_activeId)) _tabs[_activeId].CoreWebView2.Stop(); }
        public void Back() { if (_activeId != -1 && _tabs.ContainsKey(_activeId) && _tabs[_activeId].CoreWebView2.CanGoBack) _tabs[_activeId].GoBack(); }
        public void Forward() { if (_activeId != -1 && _tabs.ContainsKey(_activeId) && _tabs[_activeId].CoreWebView2.CanGoForward) _tabs[_activeId].GoForward(); }

        public async void ZoomIn() { if (_activeId != -1 && _tabs.ContainsKey(_activeId)) { _zoomLevel += 0.1; await _tabs[_activeId].CoreWebView2.ExecuteScriptAsync($"document.body.style.zoom='{_zoomLevel}'"); } }
        public async void ZoomOut() { if (_activeId != -1 && _tabs.ContainsKey(_activeId)) { _zoomLevel = Math.Max(0.1, _zoomLevel - 0.1); await _tabs[_activeId].CoreWebView2.ExecuteScriptAsync($"document.body.style.zoom='{_zoomLevel}'"); } }
        public async void ResetZoom() { if (_activeId != -1 && _tabs.ContainsKey(_activeId)) { _zoomLevel = 1.0; await _tabs[_activeId].CoreWebView2.ExecuteScriptAsync("document.body.style.zoom='1.0'"); } }
        public async void HardReload() { if (_activeId != -1 && _tabs.ContainsKey(_activeId)) await _tabs[_activeId].CoreWebView2.ExecuteScriptAsync("location.reload(true);"); }
        public async void Print() { if (_activeId != -1 && _tabs.ContainsKey(_activeId)) await _tabs[_activeId].CoreWebView2.ExecuteScriptAsync("window.print();"); }
        public void ViewSource() { if (_activeId != -1 && _tabs.ContainsKey(_activeId)) _tabs[_activeId].CoreWebView2.OpenDevToolsWindow(); }

        public void HandleContextAction(string type, int id)
        {
            if (type == "close") CloseTab(id);
            else Logger.Info($"Context {type} on tab {id} (TODO)");
        }

        private void SendToJs(object data) => _shellCore.PostWebMessageAsJson(JsonSerializer.Serialize(data));
    }
}