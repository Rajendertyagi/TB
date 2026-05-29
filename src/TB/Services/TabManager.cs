using TB.Models;

namespace TB.Services;

public sealed class TabManager : ITabManager
{
    private readonly List<BrowserTab> _tabs = [];

    public IReadOnlyList<BrowserTab> Tabs => _tabs;

    public BrowserTab? ActiveTab { get; private set; }

    public BrowserTab AddTab()
    {
        var tab = new BrowserTab();

        _tabs.Add(tab);

        ActiveTab = tab;

        return tab;
    }

    public void CloseTab(Guid id)
    {
        var tab = _tabs.FirstOrDefault(x => x.Id == id);

        if (tab is null)
        {
            return;
        }

        _tabs.Remove(tab);

        ActiveTab = _tabs.LastOrDefault();
    }

    public void SetActiveTab(Guid id)
    {
        ActiveTab = _tabs.FirstOrDefault(x => x.Id == id);
    }
}
