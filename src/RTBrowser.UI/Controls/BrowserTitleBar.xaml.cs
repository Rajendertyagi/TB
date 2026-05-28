using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RTBrowser.UI.Controls
{
    public partial class BrowserTitleBar : UserControl
    {
        public BrowserTitleBar()
        {
            InitializeComponent();

            Loaded += BrowserTitleBar_Loaded;
        }

        private void BrowserTitleBar_Loaded(object sender, RoutedEventArgs e)
        {
            ConfigureTitleBar();
        }

        private void ConfigureTitleBar()
        {
            // Reserved for future:
            // - snap layout integration
            // - custom window controls
            // - window button behavior
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            if (e.ClickCount == 2)
            {
                ToggleWindowState();
                return;
            }

            DragWindow();
        }

        private void DragWindow()
        {
            Window parentWindow = Window.GetWindow(this);

            if (parentWindow == null)
                return;

            try
            {
                parentWindow.DragMove();
            }
            catch
            {
                // Ignore drag exceptions
            }
        }

        private void ToggleWindowState()
        {
            Window parentWindow = Window.GetWindow(this);

            if (parentWindow == null)
                return;

            parentWindow.WindowState =
                parentWindow.WindowState == WindowState.Maximized
                    ? WindowState.Normal
                    : WindowState.Maximized;
        }
    }
}
