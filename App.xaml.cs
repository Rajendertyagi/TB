using System;
using System.IO;
using System.Windows;
using Microsoft.UI.Xaml;

namespace TradingBrowser
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private Window m_window;

        public App()
        {
            // 1. Catch absolute earliest .NET runtime failures (Native DLL issues, missing paths)
            AppDomain.CurrentDomain.UnhandledException += (s, e) => 
                WritePanicLog("AppDomain_UnhandledException", e.ExceptionObject as Exception);

            // 2. Catch Task background multi-threaded lock crashes
            System.Threading.Tasks.TaskScheduler.UnobservedTaskException += (s, e) => {
                WritePanicLog("TaskScheduler_UnobservedTaskException", e.Exception);
                e.SetObserved();
            };

            this.InitializeComponent();

            // 3. Catch XAML parser / UI loop failures
            this.UnhandledException += (s, e) => {
                WritePanicLog("WinUI_UnhandledException", e.Exception);
                e.Handled = true; // Attempt override suppression to observe state
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

        /// <summary>
        /// Emergency disk-write routine bypassing all custom configuration managers.
        /// </summary>
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

Target Site: 
{ex?.TargetSite}

Stack Trace:
{ex?.StackTrace}

Inner Exception:
{ex?.InnerException?.Message}
{ex?.InnerException?.StackTrace}
=================================================={Environment.NewLine}";

                // Absolute fallback atomic write
                File.AppendAllText(panicFilePath, errorBlock);
            }
            catch
            {
                // Unpreventable low-level system memory locks
            }
        }
    }
}
