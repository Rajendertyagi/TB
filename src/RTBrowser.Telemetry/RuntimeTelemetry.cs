using RTBrowser.Runtime;

using System;
using System.Collections.Generic;
using System.Linq;

namespace RTBrowser.Telemetry
{
    public sealed class RuntimeTelemetry
    {
        private readonly RuntimeCoordinator _runtime;

        public RuntimeTelemetry(RuntimeCoordinator runtime)
        {
            _runtime = runtime;
        }

        public int TotalTabs =>
            _runtime.Tabs.Count;

        public int ActiveTabs =>
            CountTabs(TabState.Active);

        public int BackgroundTabs =>
            CountTabs(TabState.Background);

        public int SleepingTabs =>
            CountTabs(TabState.Sleeping);

        public int FrozenTabs =>
            CountTabs(TabState.Frozen);

        public int DestroyedTabs =>
            CountTabs(TabState.Destroyed);

        public IEnumerable<TabRuntime> GetTabs()
        {
            return _runtime.GetAllTabs();
        }

        public RuntimeSnapshot CreateSnapshot()
        {
            return new RuntimeSnapshot
            {
                Timestamp = DateTime.UtcNow,
                TotalTabs = TotalTabs,
                ActiveTabs = ActiveTabs,
                BackgroundTabs = BackgroundTabs,
                SleepingTabs = SleepingTabs,
                FrozenTabs = FrozenTabs,
                DestroyedTabs = DestroyedTabs
            };
        }

        private int CountTabs(TabState state)
        {
            return _runtime
                .GetAllTabs()
                .Count(t => t.State == state);
        }
    }

    public sealed class RuntimeSnapshot
    {
        public DateTime Timestamp { get; set; }

        public int TotalTabs { get; set; }

        public int ActiveTabs { get; set; }

        public int BackgroundTabs { get; set; }

        public int SleepingTabs { get; set; }

        public int FrozenTabs { get; set; }

        public int DestroyedTabs { get; set; }
    }
}
