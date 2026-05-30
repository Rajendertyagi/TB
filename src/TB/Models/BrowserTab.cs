namespace TB.Models;

public sealed class BrowserTab
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public string Title { get; set; } = "New Tab";

    public string Address { get; set; } = "https://www.google.com";

    public bool CanGoBack { get; set; }

    public bool CanGoForward { get; set; }

    public bool IsLoaded { get; set; }

    public bool IsCrashed { get; set; }

    public DateTime CreatedUtc { get; init; } = DateTime.UtcNow;

    public DateTime LastVisitedUtc { get; set; } = DateTime.UtcNow;

    public DateTime LastActivatedUtc { get; set; } = DateTime.UtcNow;
}
