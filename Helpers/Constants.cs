namespace TB.Helpers;

public static class Routes
{
    public const string Settings = "tb://settings";
    public const string Downloads = "tb://downloads";
    public const string Flags = "tb://flags";
    public const string DefaultHome = "https://www.google.com";
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
    public const string FeedbackUrl = "tb://feedback";
    public const string BookmarksUrl = "tb://bookmarks";
    public const string HistoryUrl = "tb://history";
    public const string DownloadsUrl = "tb://downloads";
    public const string TaskManagerUrl = "tb://taskmanager";
    public const string ClearDataUrl = "tb://cleardata";
    public const double DefaultZoom = 1.0;
    public const double MinZoom = 0.1;
    public const double MaxZoom = 5.0;
    public const double ZoomStep = 0.1;
}
