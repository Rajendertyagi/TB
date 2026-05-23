using Microsoft.UI.Xaml;
using System;

namespace MyPortableBrowser
{
    public partial class App : Application
    {
        private Window m_window;

        public App()
        {
            // This method is auto-generated behind the scenes by WinUI 3 compiler.
            // Forcing platform x64 in the workflow will guarantee it compiles correctly!
            this.InitializeComponent(); 
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            m_window = new MainWindow();
            m_window.Title = "Portable Chrome Lite";
            m_window.Activate();
        }
    }
}
