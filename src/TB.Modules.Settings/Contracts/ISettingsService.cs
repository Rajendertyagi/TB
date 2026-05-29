using TB.Modules.Settings.Models;

namespace TB.Modules.Settings.Contracts;

public interface ISettingsService
{
    ApplicationSettings Load();
    void Save(ApplicationSettings settings);
}
