using RTBrowser.Core;

using System;
using System.IO;
using System.Text.Json;

namespace RTBrowser.Services
{
    public static class WindowStateService
    {
        private static readonly string StateDirectory =
            Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Runtime");

        private static readonly string StateFilePath =
            Path.Combine(
                StateDirectory,
                "WindowState.json");

        public static WindowStateModel Load()
        {
            try
            {
                if (!File.Exists(StateFilePath))
                {
                    return new WindowStateModel();
                }

                string json =
                    File.ReadAllText(StateFilePath);

                var state =
                    JsonSerializer.Deserialize<WindowStateModel>(json);

                return state
                    ?? new WindowStateModel();
            }
            catch
            {
                return new WindowStateModel();
            }
        }

        public static void Save(
            WindowStateModel state)
        {
            try
            {
                Directory.CreateDirectory(StateDirectory);

                string json =
                    JsonSerializer.Serialize(
                        state,
                        new JsonSerializerOptions
                        {
                            WriteIndented = true
                        });

                File.WriteAllText(
                    StateFilePath,
                    json);
            }
            catch
            {
            }
        }
    }
}
