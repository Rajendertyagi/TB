using System.Windows;
using TB.Modules.Logging.Services;
using TB.Services;
using TB.Services.FeatureFlags;
using TB.ViewModels;

namespace TB;

public partial class MainWindow : Window
{
    private readonly BrowserViewModel _viewModel;
    private readonly IBrowserService _browserService;
    private readonly ITabManager _tabManager;
    private readonly IWebViewManager _webViewManager;
    private readonly IFeatureFlagService _featureFlagService;

    public MainWindow(
        BrowserViewModel viewModel,
        IBrowserService browserService,
        ITabManager tabManager,
        IWebViewManager webViewManager,
        IFeatureFlagService featureFlagService)
    {
        LifecycleLogger.Created(
            nameof(MainWindow));

        InitializeComponent();

        _viewModel = viewModel;
        _browserService = browserService;
        _tabManager = tabManager;
        _webViewManager = webViewManager;
        _featureFlagService = featureFlagService;

        DataContext = _viewModel;

        Loaded += MainWindow_Loaded;

        CommandLogger.Completed(
            "MainWindowLoadedHandlerRegistered");

        RegisterActiveTabChangedHandler();

        CommandLogger.Completed(
            "ActiveTabChangedHandlerRegistered");

        LifecycleLogger.Initialized(
            nameof(MainWindow));
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
