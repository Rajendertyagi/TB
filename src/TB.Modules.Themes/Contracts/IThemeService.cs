using TB.Modules.Themes.Models;

namespace TB.Modules.Themes.Contracts;

public interface IThemeService
{
    ThemeSettings Load();

    ThemeDefinition GetCurrentTheme();

    void Apply(string themeName);
}
