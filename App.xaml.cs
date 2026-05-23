using Microsoft.UI.Xaml;
using System;

namespace MyPortableBrowser
{
    public partial class App : Application
    {
        private Window m_window;

        public App()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.
        /// </summary>
        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            m_window = new MainWindow();
            
            // Adjusting initial window frame size to mirror a default browser state
            m_window.Title = "Portable Chrome Lite";
            
            // Activate and display the window layout
            m_window.Activate();
        }
    }
}
