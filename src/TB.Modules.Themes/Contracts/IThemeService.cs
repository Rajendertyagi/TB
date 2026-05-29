using TB.Modules.Themes.Models;

namespace TB.Modules.Themes.Contracts;

public interface IThemeService
{
    ThemeSettings Load();
    void Apply(string themeName);
}
