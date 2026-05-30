namespace TB.Services.FeatureFlags;

public sealed class FeatureFlag
{
    public string Name { get; set; } = string.Empty;

    public bool Enabled { get; set; }

    public string Description { get; set; } = string.Empty;
}