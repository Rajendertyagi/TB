
using System;
using System.Diagnostics;

namespace RTBrowser.Services
{
    public sealed class PerformanceMonitor
    {
        private readonly Stopwatch _startupStopwatch;

        public PerformanceMonitor()
        {
            _startupStopwatch = Stopwatch.StartNew();
        }

        public long GetMemoryUsageMb()
        {
            using var process = Process.GetCurrentProcess();

            return process.WorkingSet64 / 1024 / 1024;
        }

        public long GetPrivateMemoryUsageMb()
        {
            using var process = Process.GetCurrentProcess();

            return process.PrivateMemorySize64 / 1024 / 1024;
        }

        public int GetThreadCount()
        {
            using var process = Process.GetCurrentProcess();

            return process.Threads.Count;
        }

        public TimeSpan GetStartupElapsed()
        {
            return _startupStopwatch.Elapsed;
        }

        public void StopStartupTimer()
        {
            _startupStopwatch.Stop();
        }

        public string GetRuntimeSummary()
        {
            return
                $"RAM: {GetMemoryUsageMb()} MB | " +
                $"Private: {GetPrivateMemoryUsageMb()} MB | " +
                $"Threads: {GetThreadCount()} | " +
                $"Startup: {GetStartupElapsed().TotalMilliseconds:F0} ms";
        }
    }
}
