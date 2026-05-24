using Microsoft.UI.Xaml;

namespace TB
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
            this.ExtendsContentIntoTitleBar = true; // Enables native Win11 title bar styling
        }
    }
}
