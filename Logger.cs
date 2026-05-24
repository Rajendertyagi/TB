using System;
using System.IO;

namespace TradingBrowser
{
    public static class Logger
    {
        private static readonly string LogDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");

        static Logger()
        {
            try
            {
                Directory.CreateDirectory(LogDirectory);
            }
            catch { /* Protection anchor */ }
        }

        public static void WriteLog(string trackCategory, string dataLogged)
        {
            try
            {
                string logLine = $"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff}] [{trackCategory}] {dataLogged}{Environment.NewLine}";
                string todayFile = Path.Combine(LogDirectory, $"trading_session_{DateTime.UtcNow:yyyyMMdd}.log");
                File.AppendAllText(todayFile, logLine);
            }
            catch { /* Prevent disk locks from blocking the UI thread */ }
        }
    }
}
