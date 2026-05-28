using System;

namespace RTBrowser.Services
{
    public static class BrowserPaths
    {
        public static string Root =>
            AppContext.BaseDirectory;

        public static string Logs =>
            System.IO.Path.Combine(
                Root,
                "logs");

        public static string State =>
            System.IO.Path.Combine(
                Root,
                "state");

        public static string Cache =>
            System.IO.Path.Combine(
                Root,
                "cache");

        public static string Sessions =>
            System.IO.Path.Combine(
                Root,
                "sessions");

        public static string WebViewData =>
            System.IO.Path.Combine(
                Root,
                "webview");

        public static string Downloads =>
            System.IO.Path.Combine(
                Root,
                "downloads");
    }
}
