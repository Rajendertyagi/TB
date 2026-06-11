using System;
using Windows.System;

namespace TB.Input;

public interface IShortcutCommand
{
    bool Execute();
}

public readonly struct ShortcutBinding
{
    public bool Ctrl { get; }
    public bool Alt { get; }
    public bool Shift { get; }
    public VirtualKey Key { get; }
    public Func<bool> Execute { get; }

    public ShortcutBinding(VirtualKey key, bool ctrl, bool alt, bool shift, Func<bool> execute)
    {
        Key = key; Ctrl = ctrl; Alt = alt; Shift = shift; Execute = execute;
    }
}
