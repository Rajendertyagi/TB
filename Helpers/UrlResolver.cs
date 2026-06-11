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

    public static string ResolveOmnibar(string input)
    {
        var val = input.Trim();
        if (string.IsNullOrWhiteSpace(val))
            return string.Empty;
        if (Uri.TryCreate(val, UriKind.Absolute, out var uri) &&
            (uri.Scheme == "http" || uri.Scheme == "https"))
            return val;
        if (val.Contains('.') || val.Equals("localhost", StringComparison.OrdinalIgnoreCase))
            return "https://" + val;
        return Defaults.SearchEngine + Uri.EscapeDataString(val);
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
}
