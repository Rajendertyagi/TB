using System;
using System.IO;
using System.Text.Json;

namespace RTBrowser.Services
{
    public static class WindowStateService
    {
        private static readonly string _stateDirectory =
            Path.Combine(
                AppContext.BaseDirectory,
                "state");

        private static readonly string _stateFile =
            Path.Combine(
                _stateDirectory,
                "window-state.json");

        static WindowStateService()
        {
            Directory.CreateDirectory(
                _stateDirectory);
        }

        public static WindowStateModel Load()
        {
            try
            {
                if (!File.Exists(_stateFile))
                {
                    return DefaultState();
                }

                string json =
                    File.ReadAllText(_stateFile);

                WindowStateModel? state =
                    JsonSerializer.Deserialize<WindowStateModel>(
                        json);

                return
                    state
                    ??
                    DefaultState();
            }
            catch
            {
                return DefaultState();
            }
        }

        public static void Save(
            WindowStateModel state)
        {
            try
            {
                string json =
                    JsonSerializer.Serialize(
                        state,
                        new JsonSerializerOptions
                        {
                            WriteIndented = true
                        });

                File.WriteAllText(
                    _stateFile,
                    json);
            }
            catch
            {
            }
        }

        private static WindowStateModel DefaultState()
        {
            return
                new WindowStateModel
                {
                    Width = 1720,
                    Height = 920,
                    Left = 100,
                    Top = 60,
                    IsMaximized = false
                };
        }
    }
}
