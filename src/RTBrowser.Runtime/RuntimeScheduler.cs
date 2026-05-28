using RTBrowser.Services;

using System;
using System.Threading;
using System.Threading.Tasks;

namespace RTBrowser.Runtime
{
    public sealed class RuntimeScheduler : IDisposable
    {
        private readonly RuntimeCoordinator _runtimeCoordinator;

        private readonly MemoryPressureMonitor _memoryMonitor;

        private CancellationTokenSource? _cancellationTokenSource;

        private Task? _runtimeLoopTask;

        public int RuntimeTickIntervalMs { get; set; } = 5000;

        public bool IsRunning { get; private set; }

        public RuntimeScheduler(
            RuntimeCoordinator runtimeCoordinator,
            MemoryPressureMonitor memoryMonitor)
        {
            _runtimeCoordinator = runtimeCoordinator;

            _memoryMonitor = memoryMonitor;

            _memoryMonitor.MemoryPressureDetected +=
                OnMemoryPressureDetected;
        }

        public void Start()
        {
            if (IsRunning)
                return;

            IsRunning = true;

            _cancellationTokenSource =
                new CancellationTokenSource();

            _memoryMonitor.Start();

            _runtimeLoopTask =
                Task.Run(() => RuntimeLoopAsync(
                    _cancellationTokenSource.Token));
        }

        public void Stop()
        {
            if (!IsRunning)
                return;

            IsRunning = false;

            _memoryMonitor.Stop();

            _cancellationTokenSource?.Cancel();
        }

        private async Task RuntimeLoopAsync(
            CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    _runtimeCoordinator.EvaluateRuntime();

                    await Task.Delay(
                        RuntimeTickIntervalMs,
                        cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
                catch
                {
                    // Prevent scheduler crash
                }
            }
        }

        private void OnMemoryPressureDetected()
        {
            _runtimeCoordinator.HandleMemoryPressure();
        }

        public void Dispose()
        {
            Stop();

            _memoryMonitor.MemoryPressureDetected -=
                OnMemoryPressureDetected;

            _cancellationTokenSource?.Dispose();
        }
    }
}
