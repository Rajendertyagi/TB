namespace TB.Models;

public class TabState
{
    public string TabId { get; set; }
    public string Url { get; set; }
    public string Title { get; set; }
    public bool IsPinned { get; set; }
    public bool IsSuspended { get; set; } // For the "Zzz" logic
}
