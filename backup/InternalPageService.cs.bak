using System.IO;
using System.Text;
using TB.Services.FeatureFlags;

namespace TB.Services.InternalPages;

public sealed class InternalPageService
    : IInternalPageService
{
    private const string FlagsTemplate =
        "InternalPages/flags.html";

    private readonly IFeatureFlagService _featureFlagService;

    public InternalPageService(
        IFeatureFlagService featureFlagService)
    {
        _featureFlagService =
            featureFlagService;
    }

    public bool TryGetPage(
        string url,
        out string html)
    {
        html = string.Empty;

        if (!url.Equals(
                "tb://flags",
                StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var template =
            File.ReadAllText(
                FlagsTemplate);

        var flags =
            _featureFlagService.Load();

        var rows =
            new StringBuilder();

        foreach (var flag in flags)
        {
            rows.AppendLine(
                $"""
                <tr>
                    <td>{flag.Name}</td>
                    <td>
                        <input
                            type="checkbox"
                            {(flag.Enabled ? "checked" : "")}
                            onchange="onFlagChanged(this, '{flag.Name}')">
                    </td>
                    <td>{flag.Description}</td>
                </tr>
                """);
        }

        html =
            template.Replace(
                "{{FLAGS_TABLE}}",
                rows.ToString());

        return true;
    }
}