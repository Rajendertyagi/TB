using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using TB.Helpers;

namespace TB.Models;

public class ThemeDefinition
{
    [JsonPropertyName("active")]
    public string Active { get; set; } = Defaults.Theme;

    [JsonPropertyName("native")]
    public NativeTheme Native { get; set; } = new();

    [JsonPropertyName("colors")]
    public Dictionary<string, string> Colors { get; set; } = new();

    public static ThemeDefinition Load(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return new ThemeDefinition();

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };

        try
        {
            return JsonSerializer.Deserialize<ThemeDefinition>(json, options) ?? new ThemeDefinition();
        }
        catch (JsonException ex)
        {
            TB.Infrastructure.Logger.Error($"Failed to parse theme.json: {ex.Message}");
            return new ThemeDefinition();
        }
    }
}

public class NativeTheme
{
    // Nullable: If null, ThemeService will fallback to the main "colors" dictionary
    [JsonPropertyName("titleBarText")]
    public string? TitleBarText { get; set; }

    [JsonPropertyName("titleBarIconHover")]
    public string? TitleBarIconHover { get; set; }
}

