using System.Collections.ObjectModel;
using TB.Models;
using TB.Modules.Logging.Services;

namespace TB.Services;

public sealed class TabManager : ITabManager
{
    public ObservableCollection<BrowserTab> Tabs { get; } = [];

    public BrowserTab? ActiveTab { get; private set; }

    public BrowserTab AddTab()
    {
        var tab = new BrowserTab();

        Tabs.Add(tab);

        ActiveTab = tab;

        TbLogger.TabAdded(
            tab.Id,
            Tabs.Count);

        return tab;
    }

    public BrowserTab AddTab(string address)
    {
        var tab = new BrowserTab
        {
            Address = address,
            Title = address
        };

        Tabs.Add(tab);

        ActiveTab = tab;

        TbLogger.TabAdded(
            tab.Id,
            Tabs.Count);

        return tab;
    }

    public void CloseTab(Guid id)
    {
        var tab = Tabs.FirstOrDefault(x => x.Id == id);

        if (tab is null)
        {
            return;
        }

        Tabs.Remove(tab);

        ActiveTab = Tabs.LastOrDefault();

        TbLogger.TabClosed(
            id,
            Tabs.Count);
    }

    public void SetActiveTab(Guid id)
    {
        ActiveTab = Tabs.FirstOrDefault(x => x.Id == id);

        if (ActiveTab is null)
        {
            return;
        }

        TbLogger.TabActivated(
            ActiveTab.Id,
            ActiveTab.Address);
    }
}
