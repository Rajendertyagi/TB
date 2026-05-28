using System;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace RTBrowser.UI.Controls
{
    public partial class NavigationBar : UserControl
    {
        public event Action<string>? NavigateRequested;

        public event Action? BackRequested;

        public event Action? ForwardRequested;

        public event Action? RefreshRequested;

        public NavigationBar()
        {
            InitializeComponent();

            AddressBar.KeyDown +=
                OnAddressBarKeyDown;

            AddressBar.GotFocus +=
                OnAddressBarFocused;

            AddressBar.LostFocus +=
                OnAddressBarLostFocus;

            AddressBar.PreviewMouseLeftButtonDown +=
                OnAddressBarMouseDown;

            BackButton.Click +=
                (_, _) => BackRequested?.Invoke();

            ForwardButton.Click +=
                (_, _) => ForwardRequested?.Invoke();

            RefreshButton.Click +=
                (_, _) => RefreshRequested?.Invoke();
        }

        public void SetAddress(
            string url)
        {
            if (AddressBar.IsFocused)
            {
                return;
            }

            AddressBar.Text = url;
        }

        private void OnAddressBarFocused(
            object sender,
            System.Windows.RoutedEventArgs e)
        {
            OmniboxBorder.BorderBrush =
                Brush("#4C8DFF");

            OmniboxBorder.Background =
                Brush("#1A1C20");

            AddressBar.SelectAll();
        }

        private void OnAddressBarLostFocus(
            object sender,
            System.Windows.RoutedEventArgs e)
        {
            OmniboxBorder.BorderBrush =
                Brush("#24262B");

            OmniboxBorder.Background =
                Brush("#17181B");
        }

        private void OnAddressBarMouseDown(
            object sender,
            MouseButtonEventArgs e)
        {
            if (AddressBar.IsKeyboardFocusWithin)
            {
                return;
            }

            e.Handled = true;

            AddressBar.Focus();
        }

        private void OnAddressBarKeyDown(
            object sender,
            KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
            {
                return;
            }

            string input =
                AddressBar.Text.Trim();

            if (string.IsNullOrWhiteSpace(input))
            {
                return;
            }

            NavigateRequested?.Invoke(input);
        }

        private Brush Brush(
            string hex)
        {
            return
                (Brush)new BrushConverter()
                    .ConvertFrom(hex)!;
        }
    }
}
