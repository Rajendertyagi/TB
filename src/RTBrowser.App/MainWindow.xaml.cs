using RTBrowser.Runtime;
using RTBrowser.Services;
using RTBrowser.WebView;

using System;
using System.Windows;

namespace RTBrowser.App
{
    public partial class MainWindow : Window
    {
        private readonly RuntimeCoordinator _runtime;

        private readonly RuntimeScheduler _scheduler;

        private readonly MemoryPressureMonitor _memoryMonitor;

        private readonly WebViewRuntimeManager _webViewRuntime;

        private readonly SessionManager _sessionManager;

        private readonly LoggerService _logger;

        private readonly PerformanceMonitor _performance;

        private readonly RuntimeSettings _settings;

        public MainWindow()
        {
            InitializeComponent();

            _settings = new RuntimeSettings();

            _runtime = new RuntimeCoordinator();

            _memoryMonitor = new MemoryPressureMonitor
            {
                MemoryThresholdMb =
                    _settings.MemoryPressureThresholdMb
            };

            _scheduler = new RuntimeScheduler(
                _runtime,
                _memoryMonitor);

            _webViewRuntime =
                new WebViewRuntimeManager();

            _sessionManager =
                new SessionManager();

            _logger =
                new LoggerService();

            _performance =
                new PerformanceMonitor();

            Loaded += MainWindow_Loaded;

            Closing += MainWindow_Closing;
        }

        private async void MainWindow_Loaded(
            object sender,
            RoutedEventArgs e)
        {
            try
            {
                ConfigureWindow();

                _scheduler.Start();

                var tab =
                    _runtime.CreateTab(
                        _settings.HomePage,
                        "Home");

                var binding =
                    await _webViewRuntime
                        .CreateBindingAsync(tab);

                binding.Navigate(tab.Url);

                AttachWebView(binding);

                _logger.Info(
                    "RTBrowser initialized successfully.");

                _performance.StopStartupTimer();

                _logger.Info(
                    _performance.GetRuntimeSummary());
            }
            catch (Exception exception)
            {
                _logger.Error(exception);

                MessageBox.Show(
                    exception.Message,
                    "RTBrowser Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void ConfigureWindow()
        {
            WindowStartupLocation =
                WindowStartupLocation.CenterScreen;
        }

        private void AttachWebView(
            WebViewRuntimeBinding binding)
        {
            if (!binding.IsBound)
                return;

            if (binding.Session.Host == null)
                return;

            WebViewContainer.Children.Clear();

            WebViewContainer.Children.Add(
                binding.Session.Host);
        }

        private void MainWindow_Closing(
            object? sender,
            System.ComponentModel.CancelEventArgs e)
        {
            try
            {
                _sessionManager.SaveSession(
                    _runtime.GetAllTabs());

                _scheduler.Dispose();

                _memoryMonitor.Dispose();

                _webViewRuntime.Dispose();

                _logger.Info(
                    "RTBrowser shutdown complete.");
            }
            catch (Exception exception)
            {
                _logger.Error(exception);
            }
        }
    }
}
