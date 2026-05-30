using System.Collections.ObjectModel;
using Serilog;
using TB.Models;

namespace TB.Services;

public sealed class TabManager : ITabManager
{
    public ObservableCollection<BrowserTab> Tabs { get; } = [];

    public BrowserTab? ActiveTab { get; private set; }

    public BrowserTab AddTab()
    {
        Log.Information(
            "AddTab requested. CurrentTabCount={TabCount}",
            Tabs.Count);

        var tab = new BrowserTab();

        Tabs.Add(tab);

        ActiveTab = tab;

        Log.Information(
            "Tab added. TabId={TabId}, NewTabCount={TabCount}",
            tab.Id,
            Tabs.Count);

        return tab;
    }

    public BrowserTab AddTab(string address)
    {
        Log.Information(
            "AddTab requested with address. Address={Address}, CurrentTabCount={TabCount}",
            address,
            Tabs.Count);

        var tab = new BrowserTab
        {
            Address = address,
            Title = address
        };

        Tabs.Add(tab);

        ActiveTab = tab;

        Log.Information(
            "Tab added. TabId={TabId}, Address={Address}, NewTabCount={TabCount}",
            tab.Id,
            address,
            Tabs.Count);

        return tab;
    }

    public void CloseTab(Guid id)
    {
        Log.Information(
            "CloseTab requested. TabId={TabId}",
            id);

        var tab = Tabs.FirstOrDefault(x => x.Id == id);

        if (tab is null)
        {
            Log.Warning(
                "CloseTab failed. Tab not found. TabId={TabId}",
                id);

            return;
        }

        Tabs.Remove(tab);

        ActiveTab = Tabs.LastOrDefault();

        Log.Information(
            "Tab closed. TabId={TabId}, RemainingTabs={TabCount}",
            id,
            Tabs.Count);
    }

    public void SetActiveTab(Guid id)
    {
        Log.Information(
            "SetActiveTab requested. TabId={TabId}",
            id);

        ActiveTab = Tabs.FirstOrDefault(x => x.Id == id);

        if (ActiveTab is null)
        {
            Log.Warning(
                "SetActiveTab failed. Tab not found. TabId={TabId}",
                id);

            return;
        }

        Log.Information(
            "Active tab changed. TabId={TabId}, Address={Address}",
            ActiveTab.Id,
            ActiveTab.Address);
    }
}