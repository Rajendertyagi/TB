namespace TB.Services.FeatureFlags;

public static class FeatureFlagDefaults
{
    public static IReadOnlyList<FeatureFlag> Create() =>
    [
        new()
        {
            Name = "LazyTabLoading",
            Enabled = true,
            Description =
                "Create WebViews only when tabs are activated."
        },

        new()
        {
            Name = "PreloadAdjacentTabs",
            Enabled = false,
            Description =
                "Preload neighboring tabs."
        },

        new()
        {
            Name = "AutoReloadCrashedTabs",
            Enabled = true,
            Description =
                "Automatically reload crashed WebViews."
        },

        new()
        {
            Name = "SessionAutoSave",
            Enabled = true,
            Description =
                "Automatically save session state."
        },

        new()
        {
            Name = "AggressiveMemoryCleanup",
            Enabled = false,
            Description =
                "Dispose inactive resources aggressively."
        }
    ];
}