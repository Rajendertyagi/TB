using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using TB.Models;
using TB.ViewModels;

namespace TB
{
    public sealed partial class MainWindow : Window
    {
        public MainViewModel ViewModel { get; }

        public MainWindow()
        {
            this.InitializeComponent();
            
            // Assign the ExtendsContentIntoTitleBar context
            this.ExtendsContentIntoTitleBar = true;
            this.SetTitleBar(AppTitleBar);

            ViewModel = new MainViewModel();
            ViewModel.PropertyChanged += ViewModel_PropertyChanged;

            InitializeWebView();
        }

        private async void InitializeWebView()
        {
            try
            {
                // Initializes the underlying Chromium runtime context
                await SharedWebView.EnsureCoreWebView2Async();
            }
            catch (Exception)
            {
                ViewModel.ProcessFailed();
            }
        }

        private void SharedWebView_CoreWebView2Initialized(WebView2 sender, CoreWebView2InitializedEventArgs args)
        {
            sender.CoreWebView2.ProcessFailed += (s, e) => ViewModel.ProcessFailed();
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MainViewModel.CurrentTab) && ViewModel.CurrentTab != null)
            {
                // Intercept state change to navigate the shared control safely
                if (SharedWebView.CoreWebView2 != null && SharedWebView.Source.ToString() != ViewModel.CurrentTab.Url)
                {
                    SharedWebView.Source = new Uri(ViewModel.CurrentTab.Url);
                }
            }
        }

        private void Omnibox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter && !string.IsNullOrWhiteSpace(Omnibox.Text))
            {
                ViewModel.Navigate(Omnibox.Text);
            }
        }

        private void TabStrip_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TabStrip.SelectedItem is TabState selectedTab)
            {
                ViewModel.SelectTab(selectedTab.TabId);
            }
        }

        private void CloseTab_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.DataContext is TabState tab)
            {
                ViewModel.CloseTab(tab.TabId);
            }
        }

        private void SharedWebView_NavigationCompleted(WebView2 sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs args)
        {
            if (ViewModel.CurrentTab != null && args.IsSuccess)
            {
                ViewModel.CurrentTab.Url = sender.Source.ToString();
                ViewModel.CurrentTab.Title = sender.CoreWebView2.Title;
                ViewModel.OmniboxText = sender.Source.ToString();
            }
        }

        private void GoBack_Click(object sender, RoutedEventArgs e)
        {
            if (SharedWebView.CanGoBack) SharedWebView.GoBack();
        }

        private void GoForward_Click(object sender, RoutedEventArgs e)
        {
            if (SharedWebView.CanGoForward) SharedWebView.GoForward();
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            SharedWebView.Reload();
        }
    }
}
