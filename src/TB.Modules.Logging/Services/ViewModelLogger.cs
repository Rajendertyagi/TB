using Serilog;

namespace TB.Modules.Logging.Services;

public static class ViewModelLogger
{
    public static void Created(
        string viewModelName)
    {
        Log.Information(
            "ViewModel.Created Name={ViewModel}",
            viewModelName);
    }

    public static void Initialized(
        string viewModelName)
    {
        Log.Information(
            "ViewModel.Initialized Name={ViewModel}",
            viewModelName);
    }

    public static void Activated(
        string viewModelName)
    {
        Log.Information(
            "ViewModel.Activated Name={ViewModel}",
            viewModelName);
    }

    public static void Deactivated(
        string viewModelName)
    {
        Log.Information(
            "ViewModel.Deactivated Name={ViewModel}",
            viewModelName);
    }

    public static void PropertyChanged(
        string viewModelName,
        string propertyName)
    {
        Log.Information(
            "ViewModel.PropertyChanged ViewModel={ViewModel} Property={Property}",
            viewModelName,
            propertyName);
    }

    public static void CommandExecuted(
        string viewModelName,
        string commandName)
    {
        Log.Information(
            "ViewModel.CommandExecuted ViewModel={ViewModel} Command={Command}",
            viewModelName,
            commandName);
    }

    public static void Warning(
        string viewModelName,
        string message)
    {
        Log.Warning(
            "ViewModel.Warning Name={ViewModel} Message={Message}",
            viewModelName,
            message);
    }

    public static void Disposed(
        string viewModelName)
    {
        Log.Information(
            "ViewModel.Disposed Name={ViewModel}",
            viewModelName);
    }

    public static void Failed(
        string viewModelName,
        Exception exception)
    {
        Log.Error(
            exception,
            "ViewModel.Failed Name={ViewModel}",
            viewModelName);
    }
}