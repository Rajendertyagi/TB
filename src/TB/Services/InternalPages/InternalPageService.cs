using System.IO;
using System.Text;
using TB.Modules.Logging.Services;
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
        LifecycleLogger.Created(
            nameof(InternalPageService));

        _featureFlagService =
            featureFlagService;

        LifecycleLogger.Initialized(
            nameof(InternalPageService));
    }

    public bool TryGetPage(
        string url,
        out string html)
    {
        CommandLogger.Requested(
            "InternalPageRequest");

        html = string.Empty;

        if (!url.Equals(
                "tb://flags",
                StringComparison.OrdinalIgnoreCase))
        {
            CommandLogger.Warning(
                "InternalPageRequest",
                $"No page registered for {url}");

            return false;
        }

        var template =
            File.ReadAllText(
                FlagsTemplate);

        CommandLogger.Completed(
            "FlagsTemplateLoaded");

        var flags =
            _featureFlagService.Load();

        CommandLogger.Completed(
            $"FlagsLoaded Count={flags.Count}");

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

        CommandLogger.Completed(
            "FlagsPageGenerated");

        return true;
    }
}
