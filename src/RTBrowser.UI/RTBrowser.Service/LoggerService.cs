
using System;
using System.IO;

namespace RTBrowser.Services
{
    public sealed class LoggerService
    {
        private readonly string _logDirectory;

        private readonly string _logFilePath;

        public LoggerService()
        {
            _logDirectory = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Logs");

            Directory.CreateDirectory(_logDirectory);

            _logFilePath = Path.Combine(
                _logDirectory,
                $"rtbrowser-{DateTime.Now:yyyy-MM-dd}.log");
        }

        public void Info(string message)
        {
            Write("INFO", message);
        }

        public void Warning(string message)
        {
            Write("WARNING", message);
        }

        public void Error(string message)
        {
            Write("ERROR", message);
        }

        public void Error(Exception exception)
        {
            Write(
                "ERROR",
                $"{exception.Message}{Environment.NewLine}{exception.StackTrace}");
        }

        private void Write(
            string level,
            string message)
        {
            try
            {
                var logEntry =
                    $"[{DateTime.Now:HH:mm:ss}] " +
                    $"[{level}] " +
                    $"{message}";

                File.AppendAllText(
                    _logFilePath,
                    logEntry + Environment.NewLine);
            }
            catch
            {
                // Prevent logger crashes from affecting runtime
            }
        }
    }
}
