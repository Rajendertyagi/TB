using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace TB
{
    public sealed partial class MainWindow : Window
    {
        public MainViewModel ViewModel { get; } = new MainViewModel();

        public MainWindow()
        {
            this.InitializeComponent();
            ExtendsContentIntoTitleBar = true;
            AddNewTab("https://www.google.com");
        }

        private void AddNewTab(string url)
        {
            var tab = new BrowserTab { Title = "New Tab", Url = url };
            ViewModel.Tabs.Add(tab);
        }

        private void BrowserTabs_AddTabButtonClick(TabView sender, object args) => AddNewTab("https://www.bing.com");

        private void BrowserTabs_TabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args) => ViewModel.Tabs.Remove((BrowserTab)args.Tab.DataContext);

        private void Go_Click(object sender, RoutedEventArgs e) => NavigateTo(UrlTextBox.Text);

        private void UrlTextBox_KeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter) NavigateTo(UrlTextBox.Text);
        }

        private void NavigateTo(string url)
        {
            if (BrowserTabs.SelectedItem is BrowserTab tab)
            {
                tab.Url = url.StartsWith("http") ? url : "https://" + url;
                tab.WebView.Source = new Uri(tab.Url);
            }
        }
        
        private void Back_Click(object sender, RoutedEventArgs e) => (BrowserTabs.SelectedItem as BrowserTab)?.WebView.GoBack();
        private void Forward_Click(object sender, RoutedEventArgs e) => (BrowserTabs.SelectedItem as BrowserTab)?.WebView.GoForward();
        private void Refresh_Click(object sender, RoutedEventArgs e) => (BrowserTabs.SelectedItem as BrowserTab)?.WebView.Reload();
    }
}
