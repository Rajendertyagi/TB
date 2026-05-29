namespace TB.Modules.Settings.Models;

public sealed class ApplicationSettings
{
    public string Theme { get; set; } = "System";

    public string UserDataPath { get; set; } = "UserData";

    public List<string> OpenTabs { get; set; } = [];

    public string? ActiveTab { get; set; }
}
