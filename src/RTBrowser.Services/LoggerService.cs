using System;
using System.IO;
using System.Text.Json;

namespace RTBrowser.Services
{
    public static class LoggerService
    {
        private static readonly object _lock =
            new();

        private static readonly string _logDirectory =
            Path.Combine(
                AppContext.BaseDirectory,
                "logs");

        private static readonly string _logFile =
            Path.Combine(
                _logDirectory,
                "runtime.jsonl");

        static LoggerService()
        {
            Directory.CreateDirectory(
                _logDirectory);
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

        public static void Warning(
            string category,
            string message)
        {
            Write(
                "WARNING",
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
                LogEntry entry =
                    new()
                    {
                        Timestamp =
                            DateTime.UtcNow,

                        Level =
                            level,

                        Category =
                            category,

                        Message =
                            message
                    };

                string json =
                    JsonSerializer.Serialize(entry);

                lock (_lock)
                {
                    File.AppendAllText(
                        _logFile,
                        json + Environment.NewLine);
                }
            }
            catch
            {
            }
        }

        private sealed class LogEntry
        {
            public DateTime Timestamp { get; set; }

            public string Level { get; set; } =
                string.Empty;

            public string Category { get; set; } =
                string.Empty;

            public string Message { get; set; } =
                string.Empty;
        }
    }
}
