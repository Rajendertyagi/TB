using TB.Models;

namespace TB.Services;

public interface ITabManager
{
    IReadOnlyList<BrowserTab> Tabs { get; }

    BrowserTab? ActiveTab { get; }

    BrowserTab AddTab();

    void CloseTab(Guid id);

    void SetActiveTab(Guid id);
}
