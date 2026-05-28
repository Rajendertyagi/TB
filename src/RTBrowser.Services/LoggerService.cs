using System;
using System.IO;
using System.Text.Json;

namespace RTBrowser.Services
{
    public static class LoggerService
    {
        private static readonly object LockObject = new();

        private static readonly string LogDirectory =
            Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Logs");

        private static readonly string LogFilePath =
            Path.Combine(
                LogDirectory,
                $"runtime-{DateTime.UtcNow:yyyy-MM-dd}.jsonl");

        static LoggerService()
        {
            Directory.CreateDirectory(LogDirectory);
        }

        public static void Info(
            string category,
            string message)
        {
            Write(
                "INFO",
                category,
                message);
        }

        public static void Error(
            string category,
            string message)
        {
            Write(
                "ERROR",
                category,
                message);
        }

        private static void Write(
            string level,
            string category,
            string message)
        {
            try
            {
                var payload = new
                {
                    timestamp = DateTime.UtcNow,
                    level,
                    category,
                    message
                };

                string json =
                    JsonSerializer.Serialize(payload);

                lock (LockObject)
                {
                    File.AppendAllText(
                        LogFilePath,
                        json + Environment.NewLine);
                }
            }
            catch
            {
            }
        }
    }
}
