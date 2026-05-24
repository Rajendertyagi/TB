using Microsoft.UI.Xaml;
using System;
using TB.Data;

namespace TB;

public partial class App : Application
{
    public App()
    {
        // Essential: Locate WinUI 3 binaries in Single-File mode
        Environment.SetEnvironmentVariable("MICROSOFT_WINDOWSAPPRUNTIME_BASE_DIRECTORY", AppContext.BaseDirectory);
        
        DatabaseHelper.Initialize();
        this.InitializeComponent();
    }
}
