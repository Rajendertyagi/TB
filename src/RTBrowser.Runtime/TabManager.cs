using Microsoft.Web.WebView2.Wpf;

using RTBrowser.Core;

using System;
using System.Collections.Generic;
using System.Linq;

namespace RTBrowser.Runtime
{
    public sealed class TabManager
    {
        private readonly List<BrowserTab> _tabs =
            new();

        public IReadOnlyList<BrowserTab> Tabs =>
            _tabs;

        public BrowserTab? ActiveTab { get; private set; }

        public BrowserTab CreateTab(
            WebView2 webView,
            string url)
        {
            foreach (BrowserTab existingTab in _tabs)
            {
                existingTab.IsActive = false;
            }

            BrowserTab tab =
                new()
                {
                    Title = "New Tab",
                    Url = url,
                    IsActive = true,
                    WebView = webView
                };

            _tabs.Add(tab);

            ActiveTab = tab;

            return tab;
        }

        public void SetActiveTab(
            Guid tabId)
        {
            BrowserTab? target =
                _tabs.FirstOrDefault(
                    x => x.Id == tabId);

            if (target == null)
            {
                return;
            }

            foreach (BrowserTab tab in _tabs)
            {
                tab.IsActive = false;
            }

            target.IsActive = true;

            ActiveTab = target;
        }

        public void CloseTab(
            Guid tabId)
        {
            BrowserTab? tab =
                _tabs.FirstOrDefault(
                    x => x.Id == tabId);

            if (tab == null)
            {
                return;
            }

            tab.WebView.Dispose();

            _tabs.Remove(tab);

            if (_tabs.Count == 0)
            {
                ActiveTab = null;

                return;
            }

            BrowserTab nextTab =
                _tabs.Last();

            nextTab.IsActive = true;

            ActiveTab = nextTab;
        }
    }
}
