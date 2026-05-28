using RTBrowser.Services;

using System;
using System.IO;
using System.Windows;

namespace RTBrowser.App
{
    public partial class App : Application
    {
        private LoggerService? _logger;

        private PerformanceMonitor? _performance;

        protected override void OnStartup(
            StartupEventArgs e)
        {
            base.OnStartup(e);

            InitializeServices();

            ConfigureGlobalExceptionHandling();

            _logger?.Info(
                "RTBrowser startup initialized.");
        }

        private void InitializeServices()
        {
            _logger =
                new LoggerService();

            _performance =
                new PerformanceMonitor();

            PrepareApplicationDirectories();
        }

        private void PrepareApplicationDirectories()
        {
            CreateDirectory("Logs");

            CreateDirectory("Session");

            CreateDirectory("WebViewData");

            CreateDirectory("Cache");

            CreateDirectory("Downloads");
        }

        private void CreateDirectory(
            string folderName)
        {
            string path =
                Path.Combine(
                    AppDomain.CurrentDomain.BaseDirectory,
                    folderName);

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        private void ConfigureGlobalExceptionHandling()
        {
            DispatcherUnhandledException +=
                App_DispatcherUnhandledException;

            AppDomain.CurrentDomain.UnhandledException +=
                CurrentDomain_UnhandledException;
        }

        private void App_DispatcherUnhandledException(
            object sender,
            System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            _logger?.Error(e.Exception);

            MessageBox.Show(
                e.Exception.Message,
                "RTBrowser Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(
            object sender,
            UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception exception)
            {
                _logger?.Error(exception);
            }
        }

        protected override void OnExit(
            ExitEventArgs e)
        {
            _logger?.Info(
                "RTBrowser exited successfully.");

            base.OnExit(e);
        }
    }
}
