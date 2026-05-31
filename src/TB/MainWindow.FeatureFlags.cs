using System;
using System.Text.Json;
using TB.Modules.Logging.Services;
using TB.Services.FeatureFlags;

namespace TB;

public partial class MainWindow
{
    private void HandleWebMessage(
        string json)
    {
        try
        {
            var message =
                JsonSerializer.Deserialize<FeatureFlagMessage>(
                    json);

            if (message is null)
            {
                return;
            }

            if (!string.Equals(
                    message.Type,
                    "flag-toggle",
                    StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            _featureFlagService.SetEnabled(
                message.Name,
                message.Enabled);
        }
        catch (Exception ex)
        {
            TbLogger.Exception(
                ex);
        }
    }
}
