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
            // Catch absolute earliest .NET runtime failures
            AppDomain.CurrentDomain.UnhandledException += (s, e) => 
                WritePanicLog("AppDomain_UnhandledException", e.ExceptionObject as Exception);

            // Catch Task background multi-threaded lock crashes
            System.Threading.Tasks.TaskScheduler.UnobservedTaskException += (s, e) => {
                WritePanicLog("TaskScheduler_UnobservedTaskException", e.Exception);
                e.SetObserved();
            };

            this.InitializeComponent();

            // Catch XAML parser / UI loop failures
            this.UnhandledException += (s, e) => {
                WritePanicLog("WinUI_UnhandledException", e.Exception);
                e.Handled = true; 
            };
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            try
            {
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

Source: 
{ex?.Source}

Stack Trace:
{ex?.StackTrace}

Inner Exception:
{ex?.InnerException?.Message}
{ex?.InnerException?.StackTrace}
=================================================={Environment.NewLine}";

                File.AppendAllText(panicFilePath, errorBlock);
            }
            catch
            {
                // Unpreventable low-level system locks protection anchor
            }
        }
    }
}
