using RTBrowser.Services;

using System;
using System.Windows;

namespace RTBrowser.App
{
    public partial class App : Application
    {
        private readonly SingleInstanceService _singleInstance =
            new();

        protected override void OnStartup(
            StartupEventArgs e)
        {
            base.OnStartup(e);

            bool acquired =
                _singleInstance.TryAcquire(
                    "RTBrowser");

            if (!acquired)
            {
                Shutdown();

                return;
            }

            DirectoryBootstrapper.Initialize();

            AppDomain.CurrentDomain.UnhandledException +=
                OnUnhandledException;

            DispatcherUnhandledException +=
                OnDispatcherUnhandledException;

            LoggerService.Info(
                "Application",
                "Application startup");
        }

        protected override void OnExit(
            ExitEventArgs e)
        {
            _singleInstance.Dispose();

            LoggerService.Info(
                "Application",
                "Application shutdown");

            base.OnExit(e);
        }

        private void OnUnhandledException(
            object sender,
            UnhandledExceptionEventArgs e)
        {
            LoggerService.Error(
                "Crash",
                e.ExceptionObject?.ToString()
                ?? "Unknown fatal exception");
        }

        private void OnDispatcherUnhandledException(
            object sender,
            System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            LoggerService.Error(
                "Crash",
                e.Exception.ToString());

            e.Handled = true;
        }
    }
}
