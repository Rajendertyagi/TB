using Microsoft.UI.Xaml;
using System;

namespace TB
{
    public static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            // Requirement for single-file deployment
            Environment.SetEnvironmentVariable("MICROSOFT_WINDOWSAPPRUNTIME_BASE_DIRECTORY", AppContext.BaseDirectory);

            WinRT.ComWrappersSupport.InitializeComWrappers();
            
            Application.Start((p) => new App());
        }
    }
}
