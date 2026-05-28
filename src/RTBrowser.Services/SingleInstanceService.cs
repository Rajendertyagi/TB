using System;
using System.Threading;

namespace RTBrowser.Services
{
    public sealed class SingleInstanceService
        : IDisposable
    {
        private Mutex? _mutex;

        public bool TryAcquire(
            string name)
        {
            bool createdNew;

            _mutex =
                new Mutex(
                    true,
                    $"Global\\{name}",
                    out createdNew);

            return createdNew;
        }

        public void Dispose()
        {
            try
            {
                _mutex?.ReleaseMutex();
            }
            catch
            {
            }

            _mutex?.Dispose();

            _mutex = null;
        }
    }
}
