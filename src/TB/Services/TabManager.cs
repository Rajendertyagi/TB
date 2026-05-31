using System.Collections.ObjectModel;
using TB.Models;
using TB.Modules.Logging.Services;

namespace TB.Services;

public sealed class TabManager : ITabManager
{
    public event Action<BrowserTab>? ActiveTabChanged;

    public event Action<BrowserTab>? TabCreated;

    public ObservableCollection<BrowserTab> Tabs { get; } = [];

    public BrowserTab? ActiveTab { get; private set; }

    public TabManager()
    {
        LifecycleLogger.Created(
            nameof(TabManager));

        LifecycleLogger.Initialized(
            nameof(TabManager));
    }

    public BrowserTab AddTab()
    {
        var tab = new BrowserTab
        {
            LastActivatedUtc = DateTime.UtcNow
        };

        Tabs.Add(tab);

        TabCreated?.Invoke(
            tab);

        CommandLogger.Completed(
            "TabCreatedEvent");

        ActiveTab = tab;

        TbLogger.TabAdded(
            tab.Id,
            Tabs.Count);

        SetActiveTab(
            tab.Id);

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

        TabCreated?.Invoke(
            tab);

        CommandLogger.Completed(
            "TabCreatedEvent");

        ActiveTab = tab;

        TbLogger.TabAdded(
            tab.Id,
            Tabs.Count);

        SetActiveTab(
            tab.Id);

        return tab;
    }

    public void CloseTab(Guid id)
    {
        CommandLogger.Requested(
            "CloseTab");

        var tab = Tabs.FirstOrDefault(
            x => x.Id == id);

        if (tab is null)
        {
            CommandLogger.Warning(
                "CloseTab",
                $"Tab not found: {id}");

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

            TabCreated?.Invoke(
                newTab);

            CommandLogger.Completed(
                "TabCreatedEvent");

            ActiveTab = newTab;

            TbLogger.TabClosed(
                id,
                Tabs.Count);

            SetActiveTab(
                newTab.Id);

            CommandLogger.Completed(
                "CloseTab");

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

        CommandLogger.Completed(
            "CloseTab");
    }

    public void SetActiveTab(Guid id)
    {
        CommandLogger.Requested(
            "SetActiveTab");

        ActiveTab =
            Tabs.FirstOrDefault(
                x => x.Id == id);

        if (ActiveTab is null)
        {
            CommandLogger.Warning(
                "SetActiveTab",
                $"Tab not found: {id}");

            return;
        }

        ActiveTab.LastActivatedUtc =
            DateTime.UtcNow;

        ActiveTabChanged?.Invoke(
            ActiveTab);

        CommandLogger.Completed(
            "ActiveTabChangedEvent");

        TbLogger.TabActivated(
            ActiveTab.Id,
            ActiveTab.Address);

        CommandLogger.Completed(
            "SetActiveTab");
    }
}


