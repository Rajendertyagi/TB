using System;

namespace RTBrowser.Runtime
{
    public sealed class RuntimeSettings
    {
        // =========================================
        // STARTUP
        // =========================================

        public bool LazySessionRestore { get; set; } = true;

        public bool RestorePreviousSession { get; set; } = true;

        public bool RestoreNavigationHistory { get; set; } = true;

        // =========================================
        // TAB MANAGEMENT
        // =========================================

        public bool EnableSleepingTabs { get; set; } = true;

        public bool EnableAggressiveTabUnloading { get; set; } = true;

        public bool PrioritizeActivePane { get; set; } = true;

        public TimeSpan TabSleepTimeout { get; set; }
            = TimeSpan.FromMinutes(5);

        public int MaximumActiveWebViews { get; set; } = 6;

        // =========================================
        // MEMORY
        // =========================================

        public long MemoryPressureThresholdMb { get; set; }
            = 2500;

        public bool EnableMemoryPressureCleanup { get; set; } = true;

        // =========================================
        // NETWORK
        // =========================================

        public bool PauseBackgroundNetworking { get; set; } = true;

        public bool PauseBackgroundAudio { get; set; } = true;

        // =========================================
        // PERFORMANCE
        // =========================================

        public bool EnableWebViewPooling { get; set; } = true;

        public bool EnableRuntimeTelemetry { get; set; } = true;

        public bool EnableAsyncInitialization { get; set; } = true;

        // =========================================
        // UI
        // =========================================

        public bool UseOverlayScrollbars { get; set; } = true;

        public bool EnableCompactMode { get; set; } = true;

        public bool EnableMinimalAnimations { get; set; } = true;

        // =========================================
        // SEARCH
        // =========================================

        public string DefaultSearchProvider { get; set; }
            = "Google";

        public string HomePage { get; set; }
            = "https://www.google.com";

        // =========================================
        // THEMES
        // =========================================

        public string Theme { get; set; }
            = "DarkGraphite";
    }
}
