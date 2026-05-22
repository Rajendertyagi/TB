using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Web.WebView2.Wpf;

namespace MinimalBrowser
{
    public partial class MainWindow : Window
    {
        private List<(TabItem tab, WebView2 webView)> tabs = new List<(TabItem, WebView2)>();
        private int currentTabIndex = -1;

        public MainWindow()
        {
            InitializeComponent();
            
            // Window control buttons
            MinimizeButton.Click += (s, e) => this.WindowState = WindowState.Minimized;
            MaximizeButton.Click += (s, e) => this.WindowState = this.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            CloseButton.Click += (s, e) => this.Close();
            
            // Navigation buttons
            HomeButton.Click += (s, e) => NavigateToHome();
            BackButton.Click += (s, e) => { if (GetCurrentWebView()?.CanGoBack == true) GetCurrentWebView().GoBack(); };
            ForwardButton.Click += (s, e) => { if (GetCurrentWebView()?.CanGoForward == true) GetCurrentWebView().GoForward(); };
            RefreshButton.Click += (s, e) => GetCurrentWebView()?.Reload();
            
            // Add first tab
            AddNewTab();
        }

        private WebView2 GetCurrentWebView()
        {
            if (currentTabIndex >= 0 && currentTabIndex < tabs.Count)
                return tabs[currentTabIndex].webView;
            return null;
        }

        private void AddNewTab()
        {
            var tab = new TabItem
            {
                Header = "New Tab",
                MinWidth = 100,
                MaxWidth = 200
            };

            var closeButton = new Button
            {
                Content = "×",
                Width = 20,
                Height = 20,
                Margin = new Thickness(5, 0, 0, 0),
                Padding = new Thickness(0),
                FontSize = 14,
                FontWeight = FontWeights.Bold
            };
            closeButton.Click += (s, e) => CloseTab(tab);

            var headerPanel = new StackPanel { Orientation = Orientation.Horizontal };
            var headerText = new TextBlock
            {
                Text = "New Tab",
                VerticalAlignment = VerticalAlignment.Center
            };
            headerPanel.Children.Add(headerText);
            headerPanel.Children.Add(closeButton);
            tab.Header = headerPanel;

            var webView = new WebView2();
            tab.Content = webView;

            tabs.Add((tab, webView));
            TabControl.Items.Add(tab);
            TabControl.SelectedItem = tab;
            currentTabIndex = tabs.Count - 1;

            InitializeWebViewAsync(webView, headerText, tab);
        }

        private async void InitializeWebViewAsync(WebView2 webView, TextBlock headerText, TabItem tab)
        {
            await webView.EnsureCoreWebView2Async(null);
            webView.CoreWebView2.Navigate("https://www.google.com");

            webView.CoreWebView2.NavigationStarting += (s, e) =>
            {
                Dispatcher.Invoke(() =>
                {
                    if (tabs.Count > 0 && currentTabIndex >= 0 && tabs[currentTabIndex].webView == webView)
                        AddressBar.Text = e.Uri;
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

        private void CloseTab(TabItem tabToClose)
        {
            int index = -1;
            for (int i = 0; i < tabs.Count; i++)
            {
                if (tabs[i].tab == tabToClose)
                {
                    index = i;
                    break;
                }
            }

            if (index >= 0)
            {
                tabs[index].webView?.Dispose();
                TabControl.Items.Remove(tabToClose);
                tabs.RemoveAt(index);

                if (tabs.Count == 0)
                {
                    AddNewTab();
                }
                else if (index == currentTabIndex && index < tabs.Count)
                {
                    TabControl.SelectedItem = tabs[index].tab;
                }
                else if (index == currentTabIndex && index == tabs.Count)
                {
                    currentTabIndex = tabs.Count - 1;
                    TabControl.SelectedItem = tabs[currentTabIndex].tab;
                }
                else if (index < currentTabIndex)
                {
                    currentTabIndex--;
                }
            }
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TabControl.SelectedItem is TabItem selectedTab)
            {
                for (int i = 0; i < tabs.Count; i++)
                {
                    if (tabs[i].tab == selectedTab)
                    {
                        currentTabIndex = i;
                        var webView = tabs[i].webView;
                        AddressBar.Text = webView.Source?.ToString() ?? "";
                        
                        // Show the selected webview, hide others
                        foreach (var t in tabs)
                        {
                            if (t.tab == selectedTab)
                                t.webView.Visibility = Visibility.Visible;
                            else
                                t.webView.Visibility = Visibility.Collapsed;
                        }
                        break;
                    }
                }
            }
        }

        private void NewTabButton_Click(object sender, RoutedEventArgs e)
        {
            AddNewTab();
        }

        private void NavigateToHome()
        {
            GetCurrentWebView()?.Navigate("https://www.google.com");
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
                    
                    GetCurrentWebView()?.Navigate(url);
                }
            }
        }
    }
}
