using System;
using System.Diagnostics;

namespace RTBrowser.Services
{
    public sealed class RuntimeTrace
        : IDisposable
    {
        private readonly string _category;
        private readonly string _operation;
        private readonly Stopwatch _timer;
        private bool _completed;

        internal RuntimeTrace(
            string category,
            string operation)
        {
            _category =
                category;

            _operation =
                operation;

            _timer =
                Stopwatch.StartNew();

            LoggerService.Info(
                _category,
                $"ENTER | {_operation}");
        }

        public void Step(
            string step)
        {
            LoggerService.Info(
                _category,
                $"STEP | {_operation} | {step}");
        }

        public void Success()
        {
            if (_completed)
            {
                return;
            }

            _completed = true;

            _timer.Stop();

            LoggerService.Info(
                _category,
                $"SUCCESS | {_operation} | {_timer.ElapsedMilliseconds}ms");
        }

        public void Fail(
            Exception ex)
        {
            if (_completed)
            {
                return;
            }

            _completed = true;

            _timer.Stop();

            LoggerService.Error(
                _category,
                $"FAIL | {_operation} | {_timer.ElapsedMilliseconds}ms");

            LoggerService.Error(
                _category,
                ex.ToString());
        }

        public void Dispose()
        {
            if (!_completed)
            {
                Success();
            }
        }
    }
}
