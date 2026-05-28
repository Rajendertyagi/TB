using System.Windows;
using System.Windows.Input;

namespace RTBrowser.App
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            ConfigureWindow();
        }

        private void ConfigureWindow()
        {
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            try
            {
                DragMove();
            }
            catch
            {
                // Ignore drag exceptions
            }
        }
    }
}
