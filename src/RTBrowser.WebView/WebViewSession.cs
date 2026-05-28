namespace RTBrowser.WebView
{
    public sealed class WebViewSession
    {
        public WebViewHost? Host { get; private set; }

        public bool IsAttached =>
            Host != null;

        public void Attach(
            WebViewHost host)
        {
            Host = host;
        }

        public WebViewHost? Detach()
        {
            var detachedHost = Host;

            Host = null;

            return detachedHost;
        }
    }
}
