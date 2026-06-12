using System;
using System.Text.RegularExpressions;

namespace TB.Helpers;

public static class UrlResolver
{
    public static bool IsInternalUrl(string url) =>
        url.StartsWith("tb://", StringComparison.OrdinalIgnoreCase);

    public static string Resolve(string url, string wwwrootPath)
    {
        if (!IsInternalUrl(url))
            return url;

        return url.ToLowerInvariant() switch
        {
            Routes.Settings => Path.Combine(wwwrootPath, "settings.html"),
            Routes.Downloads => Path.Combine(wwwrootPath, "downloads.html"),
            Routes.Flags => Path.Combine(wwwrootPath, "flags.html"),
            _ => Path.Combine(wwwrootPath, "404.html")
        };
    }

    public static string GetTabTitle(string url)
    {
        return url.ToLowerInvariant() switch
        {
            Routes.Settings => "Settings",
            Routes.Downloads => "Downloads",
            Routes.Flags => "Flags",
            _ => "New Tab"
        };
    }

    /// <summary>
    /// Omnibox Parser — converts raw user input into a valid navigable URI.
    /// </summary>
    public static string ParseInput(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return Defaults.HomeUrl;

        input = input.Trim();

        if (input.StartsWith("tb://", StringComparison.OrdinalIgnoreCase))
            return input;

        if (input.StartsWith("file:///", StringComparison.OrdinalIgnoreCase))
            return input;

        if (input.Length > 2 && input[1] == ':' && (input[2] == '\\' || input[2] == '/'))
            return $"file:///{input.Replace('\\', '/')}";

        if (input.Contains("://"))
            return input;

        bool hasSpaces = input.Contains(' ');
        bool hasDots = input.Contains('.');
        bool isIpAddress = Regex.IsMatch(input, @"^\d{1,3}(\.\d{1,3}){3}(:\d+)?$");

        if (hasSpaces || (!hasDots && !isIpAddress))
            return $"{Defaults.SearchEngine}{Uri.EscapeDataString(input)}";

        return $"https://{input}";
    }
}
