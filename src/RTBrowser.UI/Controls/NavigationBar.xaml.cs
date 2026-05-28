using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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

            AddressBar.KeyDown += OnAddressBarKeyDown;

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
            AddressBar.Text = url;
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
    }
}
