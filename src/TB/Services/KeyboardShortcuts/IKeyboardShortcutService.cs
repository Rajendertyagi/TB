using System.Windows.Input;

namespace TB.Services.KeyboardShortcuts;

public interface IKeyboardShortcutService
{
    bool Handle(
        Key key,
        ModifierKeys modifiers);
}
