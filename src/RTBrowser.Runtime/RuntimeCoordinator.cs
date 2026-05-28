using System;
using System.Collections.Generic;

namespace RTBrowser.Runtime
{
    public sealed class RuntimeCoordinator
    {
        private readonly Dictionary<Guid, TabRuntime> _tabs;

        public RuntimeCoordinator()
        {
            _tabs = new Dictionary<Guid, TabRuntime>();
        }

        public IReadOnlyDictionary<Guid, TabRuntime> Tabs
            => _tabs;

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

            return tab;
        }

        public void CloseTab(Guid tabId)
        {
            if (!_tabs.ContainsKey(tabId))
                return;

            _tabs.Remove(tabId);
        }

        public void ActivateTab(Guid tabId)
        {
            if (!_tabs.TryGetValue(tabId, out var tab))
                return;

            tab.Activate();
        }

        public void EvaluateRuntime()
        {
            foreach (var tab in _tabs.Values)
            {
                if (tab.State == TabState.Background)
                {
                    tab.Sleep();
                }
            }
        }

        public void HandleMemoryPressure()
        {
            foreach (var tab in _tabs.Values)
            {
                if (tab.State == TabState.Sleeping)
                {
                    tab.Freeze();
                }
            }
        }

        public IEnumerable<TabRuntime> GetAllTabs()
        {
            return _tabs.Values;
        }
    }
}
