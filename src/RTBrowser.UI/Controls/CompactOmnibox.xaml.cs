using System;
using System.Windows.Controls;
using System.Windows.Input;

namespace RTBrowser.UI.Controls
{
    public partial class CompactOmnibox : UserControl
    {
        public event Action<string>? NavigateRequested;

        public CompactOmnibox()
        {
            InitializeComponent();

            AddressBar.KeyDown += OnAddressBarKeyDown;
        }

        public void SetAddress(
            string address)
        {
            AddressBar.Text = address;
        }

        private void OnAddressBarKeyDown(
            object sender,
            KeyEventArgs e)
        {
            if (e.Key != Key.Enter)
                return;

            string input =
                AddressBar.Text.Trim();

            if (string.IsNullOrWhiteSpace(input))
                return;

            NavigateRequested?.Invoke(input);
        }
    }
}
