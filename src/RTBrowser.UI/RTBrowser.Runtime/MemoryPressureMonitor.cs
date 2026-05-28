using System;
using System.Diagnostics;
using System.Timers;

namespace RTBrowser.Runtime
{
    public sealed class MemoryPressureMonitor : IDisposable
    {
        private readonly Timer _timer;

        public event Action? MemoryPressureDetected;

        public long MemoryThresholdMb { get; set; } = 2500;

        public int CheckIntervalMs { get; set; } = 5000;

        public MemoryPressureMonitor()
        {
            _timer = new Timer(CheckIntervalMs);

            _timer.Elapsed += OnTimerElapsed;
        }

        public void Start()
        {
            _timer.Interval = CheckIntervalMs;

            _timer.Start();
        }

        public void Stop()
        {
            _timer.Stop();
        }

        private void OnTimerElapsed(
            object? sender,
            ElapsedEventArgs e)
        {
            var currentProcess = Process.GetCurrentProcess();

            long memoryUsageMb =
                currentProcess.WorkingSet64 / 1024 / 1024;

            if (memoryUsageMb >= MemoryThresholdMb)
            {
                MemoryPressureDetected?.Invoke();
            }
        }

        public void Dispose()
        {
            _timer.Stop();

            _timer.Dispose();
        }
    }
}
