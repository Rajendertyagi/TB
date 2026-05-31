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

public static void NavigationStarted(string url)
{
    Log.Information(
        "{Event} Url={Url}",
        TbLogEvents.Navigation.Started,
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

public static void WebViewInitialized()
{
    Log.Information(
        "{Event}",
        TbLogEvents.WebView.Initialized);
}

public static void WebViewCreated(
    Guid tabId,
    int loadedTabCount)
{
    Log.Information(
        "{Event} TabId={TabId} LoadedTabs={LoadedTabs}",
        TbLogEvents.WebView.Created,
        tabId,
        loadedTabCount);
}

public static void WebViewReused(
    Guid tabId)
{
    Log.Information(
        "{Event} TabId={TabId}",
        TbLogEvents.WebView.Reused,
        tabId);
}

public static void WebViewDisposed(
    Guid tabId,
    int loadedTabCount)
{
    Log.Information(
        "{Event} TabId={TabId} LoadedTabs={LoadedTabs}",
        TbLogEvents.WebView.Disposed,
        tabId,
        loadedTabCount);
}

public static void WebViewProcessFailed(string reason)
{
    Log.Error(
        "{Event} Reason={Reason}",
        TbLogEvents.WebView.ProcessFailed,
        reason);
}

public static void FontVerification(
    string windowFont,
    string addressBarFont,
    string goButtonFont)
{
    Log.Information(
        "FontVerification Window={WindowFont} AddressBar={AddressBarFont} GoButton={GoButtonFont}",
        windowFont,
        addressBarFont,
        goButtonFont);
}

public static void Exception(Exception exception)
{
    Log.Error(
        exception,
        "{Event}",
        TbLogEvents.Exception.Unhandled);
}

}
