using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;

namespace TB
{
    public sealed partial class MainWindow : Window
    {
        public MainViewModel ViewModel { get; } = new MainViewModel();

        public MainWindow()
        {
            this.InitializeComponent();

            // Setup custom title bar
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar); // Optional: Define a UI element as the drag region if needed
        }

        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            var selectedItem = (NavigationViewItem)args.SelectedItem;
            string tag = selectedItem.Tag.ToString();

            // Logic to switch pages goes here
            // e.g., if (tag == "Dashboard") ContentFrame.Navigate(typeof(DashboardPage));
            
            ViewModel.Status = $"Navigated to {tag}";
        }
    }
}
