using System;
using System.Collections.Generic;
using Windows.System;
using TB.Helpers;
using TB.Infrastructure;
using TB.Services.Interfaces;

namespace TB.Input;

public record CommandMetadata(BrowserCommand Command, string Name, string Shortcut, string Description);

public partial class CommandRegistry
{
    private readonly Lazy<ITabManager> _tabManager;
    private readonly Lazy<INavigationService> _navigationService;

    private readonly Dictionary<(VirtualKey Key, bool Ctrl, bool Alt, bool Shift), BrowserCommand> _keyToCommandMap = [];
    private readonly Dictionary<BrowserCommand, Func<bool>> _commandToActionMap = [];
    private readonly List<CommandMetadata> _commands = [];

    public IReadOnlyList<CommandMetadata> Commands => _commands;
    public ITabManager TabManager => _tabManager.Value;

    public event Action? FocusAddressBarRequested;
    public event Action? CloseWindowRequested;
    public event Action? ToggleFullScreenRequested;
    public event Action? ToggleCommandPaletteRequested;

    public CommandRegistry(Lazy<ITabManager> tabManager, Lazy<INavigationService> navigationService)
    {
        _tabManager = tabManager ?? throw new ArgumentNullException(nameof(tabManager));
        _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
        
        RegisterActions();
        RegisterKeyMappings();
    }

    public BrowserCommand? Match(VirtualKey key, bool ctrl, bool alt, bool shift)
    {
        if (_keyToCommandMap.TryGetValue((key, ctrl, alt, shift), out var cmd))
            return cmd;

        return null;
    }

    public bool Execute(BrowserCommand command)
    {
        if (_commandToActionMap.TryGetValue(command, out var action))
        {
            return action();
        }
        return false;
    }
}
