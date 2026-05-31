using System.Windows.Input;
using TB.Modules.Logging.Services;
using TB.ViewModels;

namespace TB.Services.KeyboardShortcuts;

public sealed class KeyboardShortcutService
    : IKeyboardShortcutService
{
    private readonly BrowserViewModel _viewModel;

    public KeyboardShortcutService(
        BrowserViewModel viewModel)
    {
        _viewModel = viewModel;

        LifecycleLogger.Created(
            nameof(KeyboardShortcutService));
    }

    public bool Handle(
        Key key,
        ModifierKeys modifiers)
    {
        CommandLogger.Requested(
            "KeyboardShortcut");

        if (modifiers == ModifierKeys.Control &&
            key == Key.T)
        {
            _viewModel.AddTabCommand.Execute(
                null);

            return true;
        }

        if (modifiers == ModifierKeys.Control &&
            key == Key.W)
        {
            _viewModel.CloseTabCommand.Execute(
                _viewModel.ActiveTab);

            return true;
        }

        if (key == Key.F5)
        {
            _viewModel.RefreshCommand.Execute(
                null);

            return true;
        }

        if (modifiers == ModifierKeys.Control &&
            key == Key.R)
        {
            _viewModel.RefreshCommand.Execute(
                null);

            return true;
        }

        if (modifiers == ModifierKeys.Alt &&
            key == Key.Left)
        {
            _viewModel.BackCommand.Execute(
                null);

            return true;
        }

        if (modifiers == ModifierKeys.Alt &&
            key == Key.Right)
        {
            _viewModel.ForwardCommand.Execute(
                null);

            return true;
        }

        return false;
    }
}
