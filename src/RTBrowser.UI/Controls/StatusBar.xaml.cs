using System.Windows.Controls;
using System.Windows.Threading;

namespace RTBrowser.UI.Controls
{
    public partial class StatusBar : UserControl
    {
        private readonly DispatcherTimer _timer =
            new();

        public StatusBar()
        {
            InitializeComponent();

            _timer.Interval =
                System.TimeSpan.FromSeconds(1);

            _timer.Tick +=
                OnTimerTick;

            _timer.Start();
        }

        public void SetStatus(
            string text)
        {
            StatusText.Text =
                text.ToUpperInvariant();
        }

        private void OnTimerTick(
            object? sender,
            System.EventArgs e)
        {
            TimeText.Text =
                System.DateTime.Now
                    .ToString("HH:mm:ss");
        }
    }
}
