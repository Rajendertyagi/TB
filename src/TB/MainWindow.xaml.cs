using System.Windows;
using TB.ViewModels;

namespace TB;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        DataContext = new MainWindowViewModel();
    }
}