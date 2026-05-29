using TB.Modules.Themes.Contracts;
using TB.Modules.Themes.Models;

namespace TB.Modules.Themes.Services;

public sealed class ThemeService : IThemeService
{
    public ThemeSettings Load()
    {
        return new ThemeSettings();
    }

    public void Apply(string themeName)
    {
    }
}
