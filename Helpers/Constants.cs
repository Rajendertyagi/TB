namespace TB.Helpers;

public static class Routes
{
    // SINGLE SOURCE OF TRUTH: All internal tb:// pages live here
    public const string Settings = "tb://settings";
    public const string Downloads = "tb://downloads";
    public const string Flags = "tb://flags";
    public const string Feedback = "tb://feedback";
    public const string Bookmarks = "tb://bookmarks";
    public const string History = "tb://history";
    public const string TaskManager = "tb://taskmanager";
    public const string ClearData = "tb://cleardata";
    public const string NewTab = "tb://newtab";
}

public static class Actions
{
    public const string Navigate = "NAVIGATE";
    public const string NewTab = "NEW_TAB";
    public const string CloseTab = "CLOSE_TAB";
    public const string SwitchTab = "SWITCH_TAB";
    public const string Back = "BACK";
    public const string Forward = "FORWARD";
    public const string Reload = "RELOAD";
    public const string Stop = "STOP";
    public const string ZoomIn = "ZOOM_IN";
    public const string ZoomOut = "ZOOM_OUT";
    public const string ZoomReset = "ZOOM_RESET";
    public const string HardReload = "HARD_RELOAD";
    public const string Print = "PRINT";
    public const string ViewSource = "VIEW_SOURCE";
    public const string NewWindow = "NEW_WINDOW";
    public const string FocusTab = "FOCUS_TAB";
    public const string FocusUrl = "FOCUS_URL";
    public const string EscapePressed = "ESCAPE_PRESSED";
}

public static class Defaults
{
    public const string HomeUrl = "https://www.google.com";
    public const string SearchEngine = "https://www.google.com/search?q=";
    public const string Theme = "violet-dark";

    public const string FeedbackUrl = Routes.Feedback;
    public const string BookmarksUrl = Routes.Bookmarks;
    public const string HistoryUrl = Routes.History;
    public const string DownloadsUrl = Routes.Downloads;
    public const string TaskManagerUrl = Routes.TaskManager;
    public const string ClearDataUrl = Routes.ClearData;
    public const string NewTabUrl = Routes.NewTab;

    public const double DefaultZoom = 1.0;
    public const double MinZoom = 0.1;
    public const double MaxZoom = 5.0;
    public const double ZoomStep = 0.1;
}

public static class Layout
{
    public const double TabbarHeight = 34;
    public const double NavHeight = 34;
    public const double TabHeight = 28;
    public const double UrlbarHeight = 28;

    public const double RadiusSm = 6;
    public const double RadiusMd = 8;
    public const double RadiusLg = 20;

    public const double TabInterTabGap = 2;
    public const double TabMinWidth = 32;
    public const double TabMaxWidth = 180;
}
