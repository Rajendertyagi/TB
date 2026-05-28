using System;
using System.Collections.Generic;

namespace RTBrowser.Runtime
{
    public sealed class RuntimeCoordinator
    {
        private readonly WebViewLifecycleManager _lifecycleManager;

        private readonly Dictionary<Guid, TabRuntime> _tabs;

        public RuntimeCoordinator()
        {
            _tabs = new Dictionary<Guid, TabRuntime>();

            _lifecycleManager = new WebViewLifecycleManager();
        }

        public IReadOnlyDictionary<Guid, TabRuntime> Tabs
            => _tabs;

        public WebViewLifecycleManager LifecycleManager
            => _lifecycleManager;

        public TabRuntime CreateTab(
            string url = "about:blank",
            string title = "New Tab")
        {
            var tab = new TabRuntime
            {
                Url = url,
                Title = title
            };

            _tabs[tab.Id] = tab;

            _lifecycleManager.RegisterTab(tab);

            _lifecycleManager.ActivateTab(tab);

            return tab;
        }

        public void CloseTab(Guid tabId)
        {
            if (!_tabs.TryGetValue(tabId, out var tab))
                return;

            tab.Destroy();

            _lifecycleManager.UnregisterTab(tab);

            _tabs.Remove(tabId);
        }

        public void ActivateTab(Guid tabId)
        {
            if (!_tabs.TryGetValue(tabId, out var tab))
                return;

            _lifecycleManager.ActivateTab(tab);
        }

        public void RestoreTab(Guid tabId)
        {
            if (!_tabs.TryGetValue(tabId, out var tab))
                return;

            _lifecycleManager.RestoreTab(tab);
        }

        public void EvaluateRuntime()
        {
            _lifecycleManager.EvaluateSleepingTabs();
        }

        public void HandleMemoryPressure()
        {
            _lifecycleManager.HandleMemoryPressure();
        }

        public IEnumerable<TabRuntime> GetAllTabs()
        {
            return _tabs.Values;
        }
    }
}
