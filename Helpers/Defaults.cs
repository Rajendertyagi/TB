namespace TB.Helpers;

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
