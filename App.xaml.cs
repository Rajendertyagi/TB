using System;
using System.IO;
using Microsoft.UI.Xaml;

namespace TradingBrowser
{
    public partial class App : Application
    {
        private Window m_window;

        public App()
        {
            // 1. CRITICAL EMERGENCY PROTECTIONS (Keep these first)
            AppDomain.CurrentDomain.UnhandledException += (s, e) => 
                WritePanicLog("AppDomain_UnhandledException", e.ExceptionObject as Exception);

            System.Threading.Tasks.TaskScheduler.UnobservedTaskException += (s, e) => {
                WritePanicLog("TaskScheduler_UnobservedTaskException", e.Exception);
                e.SetObserved();
            };

            this.InitializeComponent();

            this.UnhandledException += (s, e) => {
                WritePanicLog("WinUI_UnhandledException", e.Exception);
                e.Handled = true; 
            };
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            try
            {
                // 2. DIAGNOSTIC HEARTBEAT INITIALIZATION
                LoggerService.Initialize();
                LoggerService.Info("System: Application initialization sequence started.");

                m_window = new MainWindow();
                m_window.Activate();
            }
            catch (Exception ex)
            {
                WritePanicLog("App_OnLaunched_Fatal", ex);
                throw;
            }
        }

        private static void WritePanicLog(string failurePoint, Exception ex)
        {
            try
            {
                string targetDir = AppDomain.CurrentDomain.BaseDirectory;
                string panicFilePath = Path.Combine(targetDir, "CRITICAL_BOOT_PANIC.txt");
                
                string errorBlock = $@"==================================================
[CRITICAL EMERGENCY FAULT LOG]
Time (UTC): {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}
Failure Vector: {failurePoint}
==================================================
Message: 
{ex?.Message}

Stack Trace:
{ex?.StackTrace}
=================================================={Environment.NewLine}";

                File.AppendAllText(panicFilePath, errorBlock);
            }
            catch { /* Protection anchor against I/O locks */ }
        }
    }
}
