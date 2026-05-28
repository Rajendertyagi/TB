using RTBrowser.Runtime;

using System;

namespace RTBrowser.WebView
{
    public sealed class WebViewRuntimeBinding
    {
        public Guid TabId { get; }

        public TabRuntime Runtime { get; }

        public WebViewSession Session { get; }

        public bool IsBound =>
            Session.IsAttached;

        public WebViewRuntimeBinding(
            TabRuntime runtime,
            WebViewSession session)
        {
            Runtime = runtime;

            Session = session;

            TabId = runtime.Id;
        }

        public void Attach(WebViewHost host)
        {
            Session.Attach(host);

            Runtime.Touch();
        }

        public WebViewHost? Detach()
        {
            return Session.Detach();
        }

        public void Navigate(string url)
        {
            if (!Session.IsAttached)
                return;

            Session.Host?.Navigate(url);

            Runtime.Url = url;

            Runtime.Touch();
        }

        public void Reload()
        {
            if (!Session.IsAttached)
                return;

            Session.Host?.Reload();

            Runtime.Touch();
        }

        public void GoBack()
        {
            if (!Session.IsAttached)
                return;

            Session.Host?.GoBack();

            Runtime.Touch();
        }

        public void GoForward()
        {
            if (!Session.IsAttached)
                return;

            Session.Host?.GoForward();

            Runtime.Touch();
        }

        public void Stop()
        {
            if (!Session.IsAttached)
                return;

            Session.Host?.Stop();
        }

        public void Sleep()
        {
            Runtime.Sleep();
        }

        public void Restore()
        {
            Runtime.Restore();
        }

        public void Destroy()
        {
            Runtime.Destroy();

            Session.Detach();
        }
    }
}
