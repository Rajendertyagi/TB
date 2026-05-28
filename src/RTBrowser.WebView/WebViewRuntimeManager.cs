using RTBrowser.Runtime;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RTBrowser.WebView
{
    public sealed class WebViewRuntimeManager : IDisposable
    {
        private readonly WebViewPool _pool;

        private readonly Dictionary<Guid, WebViewRuntimeBinding>
            _bindings;

        public WebViewRuntimeManager()
        {
            _pool = new WebViewPool();

            _bindings =
                new Dictionary<Guid, WebViewRuntimeBinding>();
        }

        public IReadOnlyDictionary<Guid, WebViewRuntimeBinding>
            Bindings => _bindings;

        public async Task<WebViewRuntimeBinding> CreateBindingAsync(
            TabRuntime runtime)
        {
            if (_bindings.ContainsKey(runtime.Id))
            {
                return _bindings[runtime.Id];
            }

            var host =
                await _pool.AcquireAsync();

            var session =
                new WebViewSession();

            var binding =
                new WebViewRuntimeBinding(
                    runtime,
                    session);

            binding.Attach(host);

            _bindings[runtime.Id] = binding;

            return binding;
        }

        public void DestroyBinding(Guid tabId)
        {
            if (!_bindings.TryGetValue(tabId, out var binding))
                return;

            var host = binding.Detach();

            if (host != null)
            {
                _pool.Release(host);
            }

            binding.Destroy();

            _bindings.Remove(tabId);
        }

        public void SleepBinding(Guid tabId)
        {
            if (!_bindings.TryGetValue(tabId, out var binding))
                return;

            binding.Sleep();
        }

        public async Task RestoreBindingAsync(Guid tabId)
        {
            if (!_bindings.TryGetValue(tabId, out var binding))
                return;

            if (!binding.IsBound)
            {
                var host =
                    await _pool.AcquireAsync();

                binding.Attach(host);
            }

            binding.Restore();
        }

        public WebViewRuntimeBinding? GetBinding(Guid tabId)
        {
            if (_bindings.TryGetValue(tabId, out var binding))
            {
                return binding;
            }

            return null;
        }

        public IEnumerable<WebViewRuntimeBinding>
            GetAllBindings()
        {
            return _bindings.Values.ToList();
        }

        public int ActiveBindings =>
            _bindings.Count;

        public int ActiveWebViews =>
            _pool.ActiveCount;

        public int AvailableWebViews =>
            _pool.AvailableCount;

        public void Dispose()
        {
            foreach (var binding in _bindings.Values)
            {
                var host = binding.Detach();

                if (host != null)
                {
                    _pool.Release(host);
                }
            }

            _bindings.Clear();

            _pool.Dispose();
        }
    }
}
