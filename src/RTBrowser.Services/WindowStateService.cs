using System;
using System.IO;
using System.Text.Json;

namespace RTBrowser.Services
{
    public static class WindowStateService
    {
        private static readonly string _filePath =
            Path.Combine(
                AppContext.BaseDirectory,
                "state",
                "window.json");

        public static RTBrowser.Core.WindowStateModel Load()
        {
            try
            {
                if (!File.Exists(_filePath))
                {
                    return new RTBrowser.Core.WindowStateModel();
                }

                string json =
                    File.ReadAllText(
                        _filePath);

                RTBrowser.Core.WindowStateModel? state =
                    JsonSerializer.Deserialize<RTBrowser.Core.WindowStateModel>(
                        json);

                return state ??
                       new RTBrowser.Core.WindowStateModel();
            }
            catch
            {
                return new RTBrowser.Core.WindowStateModel();
            }
        }

        public static void Save(
            RTBrowser.Core.WindowStateModel state)
        {
            try
            {
                string? directory =
                    Path.GetDirectoryName(
                        _filePath);

                if (!string.IsNullOrWhiteSpace(
                    directory))
                {
                    Directory.CreateDirectory(
                        directory);
                }

                string json =
                    JsonSerializer.Serialize(
                        state,
                        new JsonSerializerOptions
                        {
                            WriteIndented = true
                        });

                File.WriteAllText(
                    _filePath,
                    json);
            }
            catch
            {
            }
        }
    }
}
