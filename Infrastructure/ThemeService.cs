using Microsoft.UI;
using Microsoft.UI.Windowing;
using System;
using System.IO;
using System.Text.Json;

namespace TB.Infrastructure
{
    public class ThemeService
    {
        public string ThemeJson { get; private set; } = "{}";

        public ThemeService(string basePath)
        {
            string path = Path.Combine(basePath, "wwwroot", "theme.json");
            if (File.Exists(path)) ThemeJson = File.ReadAllText(path);
        }

        public void ApplyNativeTheme(AppWindow appWindow)
        {
            try
            {
                using var doc = JsonDocument.Parse(ThemeJson);
                string textColor = doc.RootElement.GetProperty("native").GetProperty("titleBarText").GetString() ?? "#FFFFFF";
                appWindow.TitleBar.ButtonForegroundColor = HexToColor(textColor);
                appWindow.TitleBar.ButtonBackgroundColor = Colors.Transparent;
                appWindow.TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;
            }
            catch (Exception ex) { Logger.Error($"Theme error: {ex.Message}"); }
        }

        private Windows.UI.Color HexToColor(string hex)
        {
            hex = hex.Replace("#", "");
            return Windows.UI.Color.FromArgb(255, Convert.ToByte(hex[0..2], 16), Convert.ToByte(hex[2..4], 16), Convert.ToByte(hex[4..6], 16));
        }
    }
}