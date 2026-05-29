using System.Collections.ObjectModel;
using TB.Models;

namespace TB.Services;

public interface ITabManager
{
    ObservableCollection<BrowserTab> Tabs { get; }

    BrowserTab? ActiveTab { get; }

    BrowserTab AddTab();

    BrowserTab AddTab(string address);

    void CloseTab(Guid id);

    void SetActiveTab(Guid id);
}
