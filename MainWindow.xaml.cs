using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Web.WebView2.Wpf;
using System.Windows.Media;

namespace MinimalBrowser
{
    public partial class MainWindow : Window
    {
        private List<(Border tabBorder, WebView2 webView, TextBlock headerText)> tabs = new List<(Border, WebView2, TextBlock)>();
        private int currentTabIndex = -1;
        private const string HomeUrl = "https://www.google.com";

        public MainWindow()
        {
            InitializeComponent();
            
            // Window control buttons are handled by Click events in XAML
            
            // Navigation buttons are handled by Click events in XAML
            
            // Add first tab
            AddNewTab();
        }

        private WebView2 GetCurrentWebView()
        {
            if (currentTabIndex >= 0 && currentTabIndex < tabs.Count)
                return tabs[currentTabIndex].webView;
            return null;
        }

        private void AddTab_Click(object sender, RoutedEventArgs e)
        {
            AddNewTab();
        }

        private void NewTabButton_Click(object sender, RoutedEventArgs e)
        {
            AddNewTab();
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TabControl.SelectedItem is Border selectedBorder)
            {
                SelectTab(selectedBorder);
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            NavigateToHome();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (GetCurrentWebView()?.CanGoBack == true)
                GetCurrentWebView().GoBack();
        }

        private void ForwardButton_Click(object sender, RoutedEventArgs e)
        {
            if (GetCurrentWebView()?.CanGoForward == true)
                GetCurrentWebView().GoForward();
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            GetCurrentWebView()?.Reload();
        }

        private void AddNewTab()
        {
            var webView = new WebView2();
            
            // Create tab UI
            var tabBorder = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(0x35, 0x36, 0x3A)),
                CornerRadius = new CornerRadius(8, 8, 0, 0),
                Padding = new Thickness(10, 5, 5, 5),
                Margin = new Thickness(2, 4, 2, 0),
                Height = 32,
                VerticalAlignment = VerticalAlignment.Bottom,
                Cursor = Cursors.Hand
            };

            var tabPanel = new StackPanel { Orientation = Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Center };
            
            var headerText = new TextBlock
            {
                Text = "New Tab",
                VerticalAlignment = VerticalAlignment.Center,
                Foreground = new SolidColorBrush(Color.FromRgb(0xE8, 0xEA, 0xED)),
                FontSize = 13,
                Margin = new Thickness(0, 0, 8, 0)
            };

            var closeBtn = new Button
            {
                Content = "×",
                Width = 18,
                Height = 18,
                Padding = new Thickness(0),
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Background = Brushes.Transparent,
                Foreground = new SolidColorBrush(Color.FromRgb(0x9A, 0xA0, 0xA6)),
                BorderThickness = new Thickness(0),
                Cursor = Cursors.Arrow,
                VerticalAlignment = VerticalAlignment.Center
            };
            closeBtn.Click += (s, e) => CloseTab(tabBorder);

            tabPanel.Children.Add(headerText);
            tabPanel.Children.Add(closeBtn);
            tabBorder.Child = tabPanel;

            // Tab selection - Fixed: Pass method reference, not call
            var mouseHandler = new MouseButtonEventHandler(Tab_MouseLeftButtonDown);
            tabBorder.AddHandler(MouseLeftButtonDownEvent, mouseHandler);
            headerText.AddHandler(MouseLeftButtonDownEvent, mouseHandler);

            tabs.Add((tabBorder, webView, headerText));
            TabControl.Items.Add(tabBorder);
            
            // Add WebView to host
            WebViewContainer.Children.Clear();
            WebViewContainer.Children.Add(webView);
            
            SelectTab(tabBorder);
            InitializeWebViewAsync(webView, headerText);
        }

        private void Tab_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border)
            {
                SelectTab(border);
            }
        }

        private void SelectTab(Border selectedTabBorder)
        {
            for (int i = 0; i < tabs.Count; i++)
            {
                if (tabs[i].tabBorder == selectedTabBorder)
                {
                    currentTabIndex = i;
                    // Update visual state
                    tabs[i].tabBorder.Background = new SolidColorBrush(Color.FromRgb(0x20, 0x21, 0x24));
                    tabs[i].headerText.Foreground = new SolidColorBrush(Color.FromRgb(0xE8, 0xEA, 0xED));
                    
                    // Show the selected webview
                    WebViewContainer.Children.Clear();
                    WebViewContainer.Children.Add(tabs[i].webView);
                    
                    AddressBar.Text = tabs[i].webView.Source?.ToString() ?? "";
                }
                else
                {
                    tabs[i].tabBorder.Background = new SolidColorBrush(Color.FromRgb(0x35, 0x36, 0x3A));
                    tabs[i].headerText.Foreground = new SolidColorBrush(Color.FromRgb(0x9A, 0xA0, 0xA6));
                }
            }
        }

        private void CloseTab(Border tabBorderToClose)
        {
            int index = -1;
            for (int i = 0; i < tabs.Count; i++)
            {
                if (tabs[i].tabBorder == tabBorderToClose)
                {
                    index = i;
                    break;
                }
            }

            if (index >= 0)
            {
                tabs[index].webView?.Dispose();
                TabControl.Items.Remove(tabBorderToClose);
                tabs.RemoveAt(index);

                if (tabs.Count == 0)
                {
                    AddNewTab();
                }
                else if (index == currentTabIndex && index < tabs.Count)
                {
                    SelectTab(tabs[index].tabBorder);
                }
                else if (index == currentTabIndex && index == tabs.Count)
                {
                    SelectTab(tabs[tabs.Count - 1].tabBorder);
                }
                else if (index < currentTabIndex)
                {
                    currentTabIndex--;
                }
            }
        }

        private async void InitializeWebViewAsync(WebView2 webView, TextBlock headerText)
        {
            await webView.EnsureCoreWebView2Async(null);
            webView.CoreWebView2.Navigate(HomeUrl);

            webView.CoreWebView2.NavigationStarting += (s, e) =>
            {
                Dispatcher.Invoke(() =>
                {
                    if (tabs.Count > 0 && currentTabIndex >= 0 && tabs[currentTabIndex].webView == webView)
                        AddressBar.Text = e.Uri.ToString();
                });
            };

            webView.CoreWebView2.NavigationCompleted += (s, e) =>
            {
                Dispatcher.Invoke(() =>
                {
                    if (tabs.Count > 0 && currentTabIndex >= 0 && tabs[currentTabIndex].webView == webView)
                    {
                        AddressBar.Text = webView.Source?.ToString() ?? "";
                        headerText.Text = webView.CoreWebView2.DocumentTitle ?? "New Tab";
                    }
                });
            };
        }

        private void NavigateToHome()
        {
            var webView = GetCurrentWebView();
            if (webView?.CoreWebView2 != null)
                webView.CoreWebView2.Navigate(HomeUrl);
        }

        private void AddressBar_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Optional: Real-time validation or suggestions
        }

        private void AddressBar_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                string url = AddressBar.Text.Trim();
                if (!string.IsNullOrEmpty(url))
                {
                    if (!url.StartsWith("http://") && !url.StartsWith("https://"))
                        url = "https://" + url;
                    
                    var webView = GetCurrentWebView();
                    if (webView?.CoreWebView2 != null)
                        webView.CoreWebView2.Navigate(url);
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Ensure initial focus
            AddressBar.Focus();
        }
    }
}
