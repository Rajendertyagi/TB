using System.Collections.ObjectModel;
using TB.Models;
using TB.Modules.Logging.Services;

namespace TB.Services;

public sealed class TabManager : ITabManager
{
    public event Action<BrowserTab>? ActiveTabChanged;

    public ObservableCollection<BrowserTab> Tabs { get; } = [];

    public BrowserTab? ActiveTab { get; private set; }

    public BrowserTab AddTab()
    {
        var tab = new BrowserTab
        {
            LastActivatedUtc = DateTime.UtcNow
        };

        Tabs.Add(tab);

        ActiveTab = tab;

        TbLogger.TabAdded(
            tab.Id,
            Tabs.Count);

        SetActiveTab(tab.Id);

        return tab;
    }

    public BrowserTab AddTab(string address)
    {
        var tab = new BrowserTab
        {
            Address = address,
            Title = address,
            LastActivatedUtc = DateTime.UtcNow
        };

        Tabs.Add(tab);

        ActiveTab = tab;

        TbLogger.TabAdded(
            tab.Id,
            Tabs.Count);

        SetActiveTab(tab.Id);

        return tab;
    }

    public void CloseTab(Guid id)
    {
        var tab = Tabs.FirstOrDefault(
            x => x.Id == id);

        if (tab is null)
        {
            return;
        }

        if (Tabs.Count == 1)
        {
            Tabs.Clear();

            var newTab = new BrowserTab
            {
                LastActivatedUtc =
                    DateTime.UtcNow
            };

            Tabs.Add(newTab);

            ActiveTab = newTab;

            TbLogger.TabClosed(
                id,
                Tabs.Count);

            SetActiveTab(
                newTab.Id);

            return;
        }

        var closingActiveTab =
            ActiveTab?.Id == id;

        Tabs.Remove(tab);

        TbLogger.TabClosed(
            id,
            Tabs.Count);

        if (closingActiveTab)
        {
            var replacementTab =
                Tabs.Last();

            SetActiveTab(
                replacementTab.Id);
        }
    }

    public void SetActiveTab(Guid id)
    {
        ActiveTab =
            Tabs.FirstOrDefault(
                x => x.Id == id);

        if (ActiveTab is null)
        {
            return;
        }

        ActiveTab.LastActivatedUtc =
            DateTime.UtcNow;

        ActiveTabChanged?.Invoke(
            ActiveTab);

        TbLogger.TabActivated(
            ActiveTab.Id,
            ActiveTab.Address);
    }
}
