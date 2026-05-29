using System;
using System.Windows;
using TB.Services;
using TB.ViewModels;

namespace TB;

public partial class MainWindow : Window
{
    private readonly BrowserViewModel _viewModel;
    private readonly IBrowserService _browserService;

    public MainWindow(
        BrowserViewModel viewModel,
        IBrowserService browserService)
    {
        InitializeComponent();

        _viewModel = viewModel;
        _browserService = browserService;

        DataContext = _viewModel;

        Loaded += MainWindow_Loaded;
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        await Browser.EnsureCoreWebView2Async();

        _browserService.Attach(Browser);

        _browserService.Navigate(_viewModel.Address);
    }

    private void GoButton_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.NavigateCommand.Execute(null);
    }
}
