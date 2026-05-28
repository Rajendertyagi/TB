using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RTBrowser.WebView
{
    public sealed class WebViewPool : IDisposable
    {
        private readonly Queue<WebViewHost> _availableHosts;

        private readonly HashSet<WebViewHost> _activeHosts;

        private readonly string _userDataRoot;

        public int MaximumPoolSize { get; set; } = 6;

        public WebViewPool()
        {
            _availableHosts = new Queue<WebViewHost>();

            _activeHosts = new HashSet<WebViewHost>();

            _userDataRoot = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "WebViewData");

            Directory.CreateDirectory(_userDataRoot);
        }

        public async Task<WebViewHost> AcquireAsync()
        {
            WebViewHost host;

            if (_availableHosts.Count > 0)
            {
                host = _availableHosts.Dequeue();
            }
            else
            {
                host = new WebViewHost();

                string userDataFolder =
                    Path.Combine(
                        _userDataRoot,
                        Guid.NewGuid().ToString());

                await host.InitializeAsync(userDataFolder);
            }

            _activeHosts.Add(host);

            return host;
        }

        public void Release(WebViewHost host)
        {
            if (host == null)
                return;

            if (!_activeHosts.Contains(host))
                return;

            _activeHosts.Remove(host);

            if (_availableHosts.Count >= MaximumPoolSize)
            {
                host.Dispose();

                return;
            }

            _availableHosts.Enqueue(host);
        }

        public void Cleanup()
        {
            while (_availableHosts.Count > MaximumPoolSize)
            {
                var host = _availableHosts.Dequeue();

                host.Dispose();
            }
        }

        public int ActiveCount =>
            _activeHosts.Count;

        public int AvailableCount =>
            _availableHosts.Count;

        public IEnumerable<WebViewHost> ActiveHosts =>
            _activeHosts.ToList();

        public void Dispose()
        {
            foreach (var host in _activeHosts)
            {
                host.Dispose();
            }

            while (_availableHosts.Count > 0)
            {
                var host = _availableHosts.Dequeue();

                host.Dispose();
            }

            _activeHosts.Clear();
        }
    }
}
