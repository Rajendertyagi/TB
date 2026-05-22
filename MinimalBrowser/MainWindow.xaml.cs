using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.WinForms;
using System;
using Windows.UI.Core;

namespace MinimalBrowser
{
    public sealed partial class MainWindow : Window
    {
        private WebView2 webView;
        private TextBox addressBar;
        private Button backButton;
        private Button forwardButton;
        private Button refreshButton;

        public MainWindow()
        {
            this.InitializeComponent();
            
            // Set window title and size
            this.Title = "Minimal Browser";
            this.SetWindowSize(1200, 800);
        }

        private void InitializeComponent()
        {
            // Create main grid with dark theme
            var mainGrid = new Grid();
            mainGrid.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                Microsoft.UI.Colors.FromArgb(255, 32, 32, 32));

            // Row definitions: Address bar row and WebView row
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(50) });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            // Create address bar panel
            var addressPanel = new Grid();
            addressPanel.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                Microsoft.UI.Colors.FromArgb(255, 48, 48, 48));
            addressPanel.Padding = new Microsoft.UI.Xaml.Thickness(8, 8, 8, 8);

            // Column definitions for address bar
            addressPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });
            addressPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });
            addressPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(40) });
            addressPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            addressPanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(8) });

            // Back button
            backButton = new Button();
            backButton.Content = "←";
            backButton.FontSize = 16;
            backButton.Width = 32;
            backButton.Height = 32;
            backButton.Click += BackButton_Click;
            backButton.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                Microsoft.UI.Colors.FromArgb(255, 64, 64, 64));
            backButton.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                Microsoft.UI.Colors.FromArgb(255, 255, 255, 255));
            Grid.SetColumn(backButton, 0);

            // Forward button
            forwardButton = new Button();
            forwardButton.Content = "→";
            forwardButton.FontSize = 16;
            forwardButton.Width = 32;
            forwardButton.Height = 32;
            forwardButton.Click += ForwardButton_Click;
            forwardButton.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                Microsoft.UI.Colors.FromArgb(255, 64, 64, 64));
            forwardButton.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                Microsoft.UI.Colors.FromArgb(255, 255, 255, 255));
            Grid.SetColumn(forwardButton, 1);

            // Refresh button
            refreshButton = new Button();
            refreshButton.Content = "⟳";
            refreshButton.FontSize = 16;
            refreshButton.Width = 32;
            refreshButton.Height = 32;
            refreshButton.Click += RefreshButton_Click;
            refreshButton.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                Microsoft.UI.Colors.FromArgb(255, 64, 64, 64));
            refreshButton.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                Microsoft.UI.Colors.FromArgb(255, 255, 255, 255));
            Grid.SetColumn(refreshButton, 2);

            // Address bar textbox
            addressBar = new TextBox();
            addressBar.PlaceholderText = "Enter URL or search...";
            addressBar.FontSize = 14;
            addressBar.Height = 32;
            addressBar.KeyDown += AddressBar_KeyDown;
            addressBar.Background = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                Microsoft.UI.Colors.FromArgb(255, 64, 64, 64));
            addressBar.Foreground = new Microsoft.UI.Xaml.Media.SolidColorBrush(
                Microsoft.UI.Colors.FromArgb(255, 255, 255, 255));
            Grid.SetColumn(addressBar, 3);

            addressPanel.Children.Add(backButton);
            addressPanel.Children.Add(forwardButton);
            addressPanel.Children.Add(refreshButton);
            addressPanel.Children.Add(addressBar);

            Grid.SetRow(addressPanel, 0);
            mainGrid.Children.Add(addressPanel);

            // Initialize WebView2
            webView = new WebView2();
            webView.CoreWebView2InitializationCompleted += WebView_CoreWebView2InitializationCompleted;
            
            Task.Run(async () =>
            {
                await webView.EnsureCoreWebView2Async(null);
                
                // Apply dark theme to WebView2
                webView.CoreWebView2.Profile.SetProperty("PreferredColorScheme", 2); // Dark theme
                
                // Navigate to default page
                webView.CoreWebView2.Navigate("https://www.google.com");
            });

            Grid.SetRow(webView, 1);
            mainGrid.Children.Add(webView);

            this.Content = mainGrid;
        }

        private async void WebView_CoreWebView2InitializationCompleted(object? sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            if (e.IsSuccess)
            {
                // Enable dark mode for DevTools
                webView.CoreWebView2.Settings.AreDevToolsEnabled = true;
                
                // Update address bar on navigation
                webView.CoreWebView2.NavigationStarting += (s, args) =>
                {
                    DispatcherQueue.TryEnqueue(() =>
                    {
                        addressBar.Text = webView.CoreWebView2.Source.ToString();
                    });
                };
            }
        }

        private void AddressBar_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                string url = addressBar.Text.Trim();
                if (!url.StartsWith("http://") && !url.StartsWith("https://"))
                {
                    // Check if it looks like a URL
                    if (url.Contains(".") && !url.Contains(" "))
                    {
                        url = "https://" + url;
                    }
                    else
                    {
                        // Search query
                        url = "https://www.google.com/search?q=" + Uri.EscapeDataString(url);
                    }
                }
                webView.CoreWebView2?.Navigate(url);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (webView.CoreWebView2?.CanGoBack == true)
            {
                webView.CoreWebView2.GoBack();
            }
        }

        private void ForwardButton_Click(object sender, RoutedEventArgs e)
        {
            if (webView.CoreWebView2?.CanGoForward == true)
            {
                webView.CoreWebView2.GoForward();
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            webView.CoreWebView2?.Reload();
        }
    }
}
