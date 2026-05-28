using System;
using System.Collections.Generic;
using System.Linq;

namespace RTBrowser.Runtime
{
    public sealed class WebViewLifecycleManager
    {
        private readonly List<TabRuntime> _tabs;

        public int MaxActiveWebViews { get; set; } = 6;

        public TimeSpan SleepThreshold { get; set; }
            = TimeSpan.FromMinutes(5);

        public WebViewLifecycleManager()
        {
            _tabs = new List<TabRuntime>();
        }

        public IReadOnlyCollection<TabRuntime> Tabs
            => _tabs.AsReadOnly();

        public void RegisterTab(TabRuntime tab)
        {
            if (tab == null)
                return;

            if (_tabs.Contains(tab))
                return;

            _tabs.Add(tab);
        }

        public void UnregisterTab(TabRuntime tab)
        {
            if (tab == null)
                return;

            _tabs.Remove(tab);
        }

        public void ActivateTab(TabRuntime tab)
        {
            if (tab == null)
                return;

            foreach (var existingTab in _tabs)
            {
                if (existingTab.Id == tab.Id)
                {
                    existingTab.Activate();
                }
                else if (existingTab.State == TabState.Active)
                {
                    existingTab.SetBackground();
                }
            }

            EnforceLimits();
        }

        public void EvaluateSleepingTabs()
        {
            var now = DateTime.UtcNow;

            foreach (var tab in _tabs)
            {
                if (tab.State == TabState.Active)
                    continue;

                if (tab.State == TabState.Destroyed)
                    continue;

                var inactiveDuration =
                    now - tab.LastAccessedAt;

                if (inactiveDuration >= SleepThreshold)
                {
                    tab.Sleep();
                }
            }
        }

        public void HandleMemoryPressure()
        {
            var candidates = _tabs
                .Where(t =>
                    t.State == TabState.Sleeping ||
                    t.State == TabState.Background)
                .OrderBy(t => t.LastAccessedAt)
                .ToList();

            foreach (var tab in candidates)
            {
                tab.Destroy();
            }
        }

        public void RestoreTab(TabRuntime tab)
        {
            if (tab == null)
                return;

            if (tab.State == TabState.Destroyed ||
                tab.State == TabState.Sleeping ||
                tab.State == TabState.Frozen)
            {
                tab.Restore();
                tab.Activate();
            }
        }

        private void EnforceLimits()
        {
            var activeTabs = _tabs
                .Where(t =>
                    t.State == TabState.Active ||
                    t.State == TabState.Visible)
                .ToList();

            if (activeTabs.Count <= MaxActiveWebViews)
                return;

            var tabsToSleep = activeTabs
                .OrderBy(t => t.LastAccessedAt)
                .Take(activeTabs.Count - MaxActiveWebViews);

            foreach (var tab in tabsToSleep)
            {
                tab.Sleep();
            }
        }
    }
}
