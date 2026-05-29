using System;
using System.Diagnostics;

namespace RTBrowser.Services
{
    public sealed class RuntimeTrace
        : IDisposable
    {
        private readonly string _category;

        private readonly string _operation;

        private readonly Stopwatch _stopwatch;

        private bool _completed;

        internal RuntimeTrace(
            string category,
            string operation)
        {
            _category =
                category;

            _operation =
                operation;

            _stopwatch =
                Stopwatch.StartNew();

            LoggerService.Info(
                _category,
                $"{_operation} ENTER");
        }

        public void Step(
            string message)
        {
            LoggerService.Info(
                _category,
                $"{_operation} | {message}");
        }

        public void Success(
            string? message = null)
        {
            if (_completed)
            {
                return;
            }

            _completed = true;

            _stopwatch.Stop();

            LoggerService.Info(
                _category,
                $"{_operation} SUCCESS ({_stopwatch.ElapsedMilliseconds}ms)"
                + (string.IsNullOrWhiteSpace(message)
                    ? string.Empty
                    : $" | {message}"));
        }

        public void Fail(
            Exception exception)
        {
            if (_completed)
            {
                return;
            }

            _completed = true;

            _stopwatch.Stop();

            LoggerService.Error(
                _category,
                $"{_operation} FAIL ({_stopwatch.ElapsedMilliseconds}ms) | {exception}");
        }

        public void Dispose()
        {
            if (_completed)
            {
                return;
            }

            Success();
        }
    }

    public static class RuntimeDiagnostics
    {
        public static RuntimeTrace Trace(
            string category,
            string operation)
        {
            return new RuntimeTrace(
                category,
                operation);
        }

        public static void Step(
            string category,
            string message)
        {
            LoggerService.Info(
                category,
                message);
        }

        public static void Success(
            string category,
            string message)
        {
            LoggerService.Info(
                category,
                $"SUCCESS | {message}");
        }

        public static void Fail(
            string category,
            Exception exception)
        {
            LoggerService.Error(
                category,
                exception.ToString());
        }
    }
}
