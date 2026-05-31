using Serilog;

namespace TB.Modules.Logging.Services;

public static class BookmarkLogger
{
    public static void Added(
        string title,
        string url)
    {
        Log.Information(
            "Bookmark.Added Title={Title} Url={Url}",
            title,
            url);
    }

    public static void Updated(
        string title,
        string url)
    {
        Log.Information(
            "Bookmark.Updated Title={Title} Url={Url}",
            title,
            url);
    }

    public static void Removed(
        string title,
        string url)
    {
        Log.Information(
            "Bookmark.Removed Title={Title} Url={Url}",
            title,
            url);
    }

    public static void Opened(
        string title,
        string url)
    {
        Log.Information(
            "Bookmark.Opened Title={Title} Url={Url}",
            title,
            url);
    }

    public static void Failed(
        string title,
        Exception exception)
    {
        Log.Error(
            exception,
            "Bookmark.Failed Title={Title}",
            title);
    }
}