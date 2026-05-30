namespace TB.Services.FeatureFlags;

public interface IFeatureFlagService
{
    IReadOnlyList<FeatureFlag> Load();

    void Save(
        IReadOnlyList<FeatureFlag> flags);

    bool IsEnabled(
        string name);

    void SetEnabled(
        string name,
        bool enabled);
}
