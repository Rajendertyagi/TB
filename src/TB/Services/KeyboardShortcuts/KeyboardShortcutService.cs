using System.Windows.Input;
using TB.Modules.Logging.Services;

namespace TB.Services.KeyboardShortcuts;

public sealed class KeyboardShortcutService
    : IKeyboardShortcutService
{
    public bool Handle(
        Key key,
        ModifierKeys modifiers)
    {
        CommandLogger.Requested(
            "KeyboardShortcut");

        return false;
    }
}
