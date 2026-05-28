using System;

namespace RTBrowser.Runtime
{
    public sealed class TabSleepPolicy
    {
        public bool EnableSleepingTabs { get; set; } = true;

        public bool EnableAggressiveUnloading { get; set; } = true;

        public bool PauseBackgroundNetworking { get; set; } = true;

        public bool PauseBackgroundAudio { get; set; } = true;

        public bool PrioritizeActivePane { get; set; } = true;

        public TimeSpan SleepAfterInactivity { get; set; }
            = TimeSpan.FromMinutes(5);

        public int MaximumActiveWebViews { get; set; } = 6;

        public long MemoryPressureThresholdMb { get; set; } = 2500;

        public bool ShouldSleep(
            TabRuntime tab,
            DateTime now)
        {
            if (!EnableSleepingTabs)
                return false;

            if (tab.State == TabState.Active)
                return false;

            if (tab.State == TabState.Destroyed)
                return false;

            TimeSpan inactiveDuration =
                now - tab.LastAccessedAt;

            return inactiveDuration >=
                   SleepAfterInactivity;
        }

        public bool ShouldUnloadUnderPressure(
            TabRuntime tab)
        {
            if (!EnableAggressiveUnloading)
                return false;

            return
                tab.State == TabState.Sleeping ||
                tab.State == TabState.Background ||
                tab.State == TabState.Frozen;
        }
    }
}
