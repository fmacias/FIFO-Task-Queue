using fmacias;
using NLog;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using Unity;

namespace WPF_Usage
{
    public partial class App : Application, IExampleModule
    {
        private IUnityContainer container;
        void App_Startup(object sender, StartupEventArgs e)
        {              
            IUnityContainer container = new UnityContainer();
        }
        void App_Exit(object sender, ExitEventArgs e)
        {
            container.Dispose();
        }

        public ExampleModule.Module Load()
        {
            container.RegisterType<ExampleModule.Module>();
            return container.Resolve<ExampleModule.Module>();
        }
    }
}
