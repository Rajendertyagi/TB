namespace TB.Models;

public class SessionState
{
    public List<TabEntry> Tabs { get; set; } = new();
    public int ActiveTabIndex { get; set; } = -1;
}

public class TabEntry
{
    public string Url { get; set; } = "";
    public string Title { get; set; } = "";
}
