using RTBrowser.Services;

using System;
using System.Threading;
using System.Windows;

namespace RTBrowser.App
{
    public partial class App : Application
    {
        private static Mutex? _mutex;

        protected override void OnStartup(
            StartupEventArgs e)
        {
            const string mutexName =
                "RTBrowser.SingleInstance";

            bool createdNew;

            _mutex = new Mutex(
                true,
                mutexName,
                out createdNew);

            if (!createdNew)
            {
                MessageBox.Show(
                    "RTBrowser is already running.",
                    "RTBrowser");

                Current.Shutdown();

                return;
            }

            LoggerService.Info(
                "Application",
                "Application startup");

            AppDomain.CurrentDomain.UnhandledException +=
                OnUnhandledException;

            base.OnStartup(e);
        }

        protected override void OnExit(
            ExitEventArgs e)
        {
            LoggerService.Info(
                "Application",
                "Application shutdown");

            _mutex?.ReleaseMutex();

            _mutex?.Dispose();

            base.OnExit(e);
        }

        private void OnUnhandledException(
            object sender,
            UnhandledExceptionEventArgs e)
        {
            LoggerService.Error(
                "Crash",
                e.ExceptionObject.ToString() ?? "Unknown crash");
        }
    }
}
