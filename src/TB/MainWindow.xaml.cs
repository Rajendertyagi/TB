using System.Windows;
using System.Windows.Input;
using TB.Modules.Logging.Services;
using TB.Services;
using TB.Services.FeatureFlags;
using TB.Services.KeyboardShortcuts;
using TB.ViewModels;

namespace TB;

public partial class MainWindow : Window
{
    private readonly BrowserViewModel _viewModel;
    private readonly IBrowserService _browserService;
    private readonly ITabManager _tabManager;
    private readonly IWebViewManager _webViewManager;
    private readonly IFeatureFlagService _featureFlagService;
    private readonly IKeyboardShortcutService _keyboardShortcutService;

    public MainWindow(
        BrowserViewModel viewModel,
        IBrowserService browserService,
        ITabManager tabManager,
        IWebViewManager webViewManager,
        IFeatureFlagService featureFlagService,
        IKeyboardShortcutService keyboardShortcutService)
    {
        LifecycleLogger.Created(
            nameof(MainWindow));

        InitializeComponent();

        _viewModel = viewModel;
        _browserService = browserService;
        _tabManager = tabManager;
        _webViewManager = webViewManager;
        _featureFlagService = featureFlagService;
        _keyboardShortcutService = keyboardShortcutService;

        DataContext = _viewModel;

        Loaded += MainWindow_Loaded;

        CommandLogger.Completed(
            "MainWindowLoadedHandlerRegistered");

        RegisterActiveTabChangedHandler();

        CommandLogger.Completed(
            "ActiveTabChangedHandlerRegistered");

        PreviewKeyDown += MainWindow_PreviewKeyDown;

        CommandLogger.Completed(
            "KeyboardShortcutHandlerRegistered");

        LifecycleLogger.Initialized(
            nameof(MainWindow));
    }

    private void MainWindow_PreviewKeyDown(
        object sender,
        KeyEventArgs e)
    {
        if (_keyboardShortcutService.Handle(
                e.Key,
                Keyboard.Modifiers))
        {
            e.Handled = true;
        }
    }

    private void GoButton_Click(
        object sender,
        RoutedEventArgs e)
    {
        CommandLogger.Requested(
            "GoButtonClick");

        _viewModel.NavigateCommand.Execute(
            null);

        CommandLogger.Completed(
            "GoButtonClick");
    }
}
