using System;
using System.Windows;
using TB.ViewModels;

namespace TB;

public partial class MainWindow : Window
{
    private readonly BrowserViewModel _viewModel;

    public MainWindow(BrowserViewModel viewModel)
    {
        InitializeComponent();

        _viewModel = viewModel;
        DataContext = _viewModel;

        Loaded += MainWindow_Loaded;
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        await Browser.EnsureCoreWebView2Async();

        Browser.Source = new Uri(_viewModel.Address);
    }

    private void GoButton_Click(object sender, RoutedEventArgs e)
    {
        if (Uri.TryCreate(_viewModel.Address, UriKind.Absolute, out var uri))
        {
            Browser.Source = uri;
        }
    }
}
