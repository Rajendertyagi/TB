using Serilog;

namespace TB.Modules.Logging.Services;

public static class BookmarkLogger
{
    public static void AddRequested(
        string title,
        string url)
    {
        Log.Information(
            "Bookmark.Add.Requested Title={Title} Url={Url}",
            title,
            url);
    }

    public static void AddCompleted(
        string title,
        string url)
    {
        Log.Information(
            "Bookmark.Add.Completed Title={Title} Url={Url}",
            title,
            url);
    }

    public static void RemoveRequested(
        string title,
        string url)
    {
        Log.Information(
            "Bookmark.Remove.Requested Title={Title} Url={Url}",
            title,
            url);
    }

    public static void RemoveCompleted(
        string title,
        string url)
    {
        Log.Information(
            "Bookmark.Remove.Completed Title={Title} Url={Url}",
            title,
            url);
    }

    public static void UpdateRequested(
        string title,
        string url)
    {
        Log.Information(
            "Bookmark.Update.Requested Title={Title} Url={Url}",
            title,
            url);
    }

    public static void UpdateCompleted(
        string title,
        string url)
    {
        Log.Information(
            "Bookmark.Update.Completed Title={Title} Url={Url}",
            title,
            url);
    }

    public static void OpenRequested(
        string title,
        string url)
    {
        Log.Information(
            "Bookmark.Open.Requested Title={Title} Url={Url}",
            title,
            url);
    }

    public static void OpenCompleted(
        string title,
        string url)
    {
        Log.Information(
            "Bookmark.Open.Completed Title={Title} Url={Url}",
            title,
            url);
    }

    public static void Loaded(
        int bookmarkCount)
    {
        Log.Information(
            "Bookmark.Loaded Count={Count}",
            bookmarkCount);
    }

    public static void Saved(
        int bookmarkCount)
    {
        Log.Information(
            "Bookmark.Saved Count={Count}",
            bookmarkCount);
    }

    public static void Failed(
        string operation,
        Exception exception)
    {
        Log.Error(
            exception,
            "Bookmark.Failed Operation={Operation}",
            operation);
    }
}