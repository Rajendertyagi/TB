using System.Text.Json;
using TB.Helpers;
using Windows.UI;

namespace TB.Models;

public class ThemeDefinition
{
    public string Active { get; set; } = "github-dark";
    public NativeTheme Native { get; set; } = new();
    public Dictionary<string, string> Colors { get; set; } = new();
    public Dictionary<string, double> Sizes { get; set; } = new();

    public class NativeTheme
    {
        public string TitleBarText { get; set; } = "#FFFFFF";
        public string TitleBarIconHover { get; set; } = "#A855F7";

        public Color TitleBarTextColor => ColorExtensions.ParseHex(TitleBarText);
        public Color TitleBarIconHoverColor => ColorExtensions.ParseHex(TitleBarIconHover);
    }

    public static ThemeDefinition Load(string json)
    {
        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var active = root.TryGetProperty("active", out var activeEl)
            ? activeEl.GetString() ?? "github-dark" : "github-dark";

        var native = new NativeTheme();
        if (root.TryGetProperty("native", out var nativeEl))
        {
            native.TitleBarText = nativeEl.TryGetProperty("titleBarText", out var tbt) ? tbt.GetString() ?? "#FFFFFF" : "#FFFFFF";
            native.TitleBarIconHover = nativeEl.TryGetProperty("titleBarIconHover", out var tih) ? tih.GetString() ?? "#A855F7" : "#A855F7";
        }

        var colors = new Dictionary<string, string>();
        if (root.TryGetProperty("colors", out var colorsEl))
        {
            foreach (var prop in colorsEl.EnumerateObject())
                colors[prop.Name] = prop.Value.GetString() ?? "";
        }

        var sizes = new Dictionary<string, double>();
        if (root.TryGetProperty("sizes", out var sizesEl))
        {
            foreach (var prop in sizesEl.EnumerateObject())
                sizes[prop.Name] = prop.Value.GetDouble();
        }

        return new ThemeDefinition { Active = active, Native = native, Colors = colors, Sizes = sizes };
    }
}
