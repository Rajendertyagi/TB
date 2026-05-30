namespace TB.Modules.Logging.Constants;

public static class TbLogEvents
{
    public static class Application
    {
        public const string Startup = "Application.Startup";
        public const string Shutdown = "Application.Shutdown";
        public const string CrashDetected = "Application.CrashDetected";
    }

    public static class Tabs
    {
        public const string Added = "Tabs.Added";
        public const string Closed = "Tabs.Closed";
        public const string Activated = "Tabs.Activated";
    }

    public static class Navigation
    {
        public const string Requested = "Navigation.Requested";
        public const string Completed = "Navigation.Completed";
        public const string Failed = "Navigation.Failed";
    }

    public static class WebView
    {
        public const string Attached = "WebView.Attached";
        public const string Initialized = "WebView.Initialized";
        public const string ProcessFailed = "WebView.ProcessFailed";
    }

    public static class Network
    {
        public const string DnsFailure = "Network.DnsFailure";
        public const string Timeout = "Network.Timeout";
        public const string HttpError = "Network.HttpError";
    }

    public static class Exception
    {
        public const string Unhandled = "Exception.Unhandled";
    }

    public static class Performance
    {
        public const string Startup = "Performance.Startup";
        public const string Navigation = "Performance.Navigation";
    }
}
