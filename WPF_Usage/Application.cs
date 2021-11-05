using System;
using System.Windows;

namespace WPF_Usage
{
    public class WPF_App: Application
    {
        [STAThread]
        public static void Main()
        {
            WPF_App app = new WPF_App();
            app.Run();
        }
    }
}
