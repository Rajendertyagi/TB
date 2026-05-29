namespace TB.Models;

public sealed class BrowserTab
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public string Title { get; set; } = "New Tab";

    public string Address { get; set; } = "https://www.google.com";
}
