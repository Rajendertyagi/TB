using Serilog;
using TB.Modules.Logging.Constants;

namespace TB.Modules.Logging.Services;

public static class TbLogger
{
    public static void ApplicationStartup()
    {
        Log.Information(
            "{Event}",
            TbLogEvents.Application.Startup);
    }

    public static void ApplicationShutdown()
    {
        Log.Information(
            "{Event}",
            TbLogEvents.Application.Shutdown);
    }

    public static void CrashDetected()
    {
        Log.Warning(
            "{Event}",
            TbLogEvents.Application.CrashDetected);
    }

    public static void TabAdded(Guid tabId, int tabCount)
    {
        Log.Information(
            "{Event} TabId={TabId} TabCount={TabCount}",
            TbLogEvents.Tabs.Added,
            tabId,
            tabCount);
    }

    public static void TabClosed(Guid tabId, int tabCount)
    {
        Log.Information(
            "{Event} TabId={TabId} TabCount={TabCount}",
            TbLogEvents.Tabs.Closed,
            tabId,
            tabCount);
    }

    public static void TabActivated(Guid tabId, string address)
    {
        Log.Information(
            "{Event} TabId={TabId} Address={Address}",
            TbLogEvents.Tabs.Activated,
            tabId,
            address);
    }

    public static void NavigationRequested(string url)
    {
        Log.Information(
            "{Event} Url={Url}",
            TbLogEvents.Navigation.Requested,
            url);
    }

    public static void NavigationCompleted(string url)
    {
        Log.Information(
            "{Event} Url={Url}",
            TbLogEvents.Navigation.Completed,
            url);
    }

    public static void NavigationFailed(
        string url,
        Exception exception)
    {
        Log.Error(
            exception,
            "{Event} Url={Url}",
            TbLogEvents.Navigation.Failed,
            url);
    }

    public static void WebViewAttached()
    {
        Log.Information(
            "{Event}",
            TbLogEvents.WebView.Attached);
    }

    public static void Exception(Exception exception)
    {
        Log.Error(
            exception,
            "{Event}",
            TbLogEvents.Exception.Unhandled);
    }
}
