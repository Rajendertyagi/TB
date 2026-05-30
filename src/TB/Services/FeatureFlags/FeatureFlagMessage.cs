namespace TB.Services.FeatureFlags;

public sealed class FeatureFlagMessage
{
    public string Type { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public bool Enabled { get; set; }
}
