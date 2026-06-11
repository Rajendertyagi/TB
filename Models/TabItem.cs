using TB.Helpers;

namespace TB.Models;

public class TabItem
{
    public int Id { get; init; }
    public string Url { get; set; } = Defaults.HomeUrl;
    public string Title { get; set; } = "New Tab";
    public double Zoom { get; set; } = Defaults.DefaultZoom;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public bool IsInternalPage { get; set; }
}
