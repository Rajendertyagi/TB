namespace RTBrowser.Core
{
    public static class BrowserConstants
    {
        // =========================================
        // APPLICATION
        // =========================================

        public const string ApplicationName =
            "RTBrowser";

        public const string DefaultHomePage =
            "https://www.google.com";

        // =========================================
        // UI
        // =========================================

        public const double TabBarHeight = 30;

        public const double OmniboxHeight = 32;

        public const double StatusBarHeight = 22;

        // =========================================
        // TAB SYSTEM
        // =========================================

        public const int MinimumTabWidth = 110;

        public const int PreferredTabWidth = 160;

        public const int MaximumTabWidth = 220;

        public const int PinnedTabWidth = 36;

        // =========================================
        // RUNTIME
        // =========================================

        public const int DefaultMaxActiveWebViews = 6;

        public const int DefaultSleepThresholdMinutes = 5;

        public const int RuntimeEvaluationIntervalMs = 5000;

        // =========================================
        // MEMORY
        // =========================================

        public const long DefaultMemoryThresholdMb = 2500;

        // =========================================
        // PERFORMANCE
        // =========================================

        public const int StartupTargetMs = 1000;

        // =========================================
        // SESSION
        // =========================================

        public const string SessionFileName =
            "session.json";

        // =========================================
        // LOGGING
        // =========================================

        public const string LogsFolderName =
            "Logs";

        // =========================================
        // SEARCH
        // =========================================

        public const string DefaultSearchProvider =
            "Google";

        // =========================================
        // THEMES
        // =========================================

        public const string DefaultTheme =
            "DarkGraphite";
    }
}
