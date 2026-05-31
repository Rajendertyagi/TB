using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TB.UI.Controls;

public partial class TitleBar : UserControl
{
    public TitleBar()
    {
        InitializeComponent();
    }

    private void TitleBar_MouseLeftButtonDown(
        object sender,
        MouseButtonEventArgs e)
    {
        var window =
            Window.GetWindow(this);

        if (window is null)
        {
            return;
        }

        if (e.ClickCount == 2)
        {
            window.WindowState =
                window.WindowState == WindowState.Maximized
                    ? WindowState.Normal
                    : WindowState.Maximized;

            return;
        }

        window.DragMove();
    }

    private void Minimize_Click(
        object sender,
        RoutedEventArgs e)
    {
        var window =
            Window.GetWindow(this);

        if (window is not null)
        {
            window.WindowState =
                WindowState.Minimized;
        }
    }

    private void Maximize_Click(
        object sender,
        RoutedEventArgs e)
    {
        var window =
            Window.GetWindow(this);

        if (window is null)
        {
            return;
        }

        window.WindowState =
            window.WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
    }

    private void Close_Click(
        object sender,
        RoutedEventArgs e)
    {
        Window.GetWindow(this)?.Close();
    }
}
