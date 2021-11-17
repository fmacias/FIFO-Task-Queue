using fmacias.Components.MVPVMModule;
using fmacias.Components.MVPVMModule.Abstract;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Unity;
using Unity.Injection;
using WpfControlLibraryDummyJobQueue;

namespace WpfAppExample
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private IUnityContainer container;
        private IModuleRegister jobQueueModule;
        void App_Startup(object sender, StartupEventArgs e)
        {
            container = new UnityContainer();
            RegisterModules();
            container.Resolve<DummyFifoJobQueue>().Show();
        }
        void App_Exit(object sender, ExitEventArgs e)
        {
            GetPresenter<DummyFifoJobQueue>().UnsubscribeAll();
            container.Dispose();
        }
        private IPresenter<T> GetPresenter<T>()
        {
            return container.Resolve<IPresenter<T>>(
                jobQueueModule.GetUnicRegistrationName(typeof(JobQueuePresenter<T>))
            );
        }
        private void RegisterModules()
        {
            RegisterDummyJobQueue();
        }
        private void RegisterDummyJobQueue()
        {
            container.RegisterType<IJobQueueModuleRegister, JobQueueModuleRegister>(
                new InjectionConstructor(container)
            );
            jobQueueModule = container.Resolve<IJobQueueModuleRegister>();
            jobQueueModule.Register<DummyFifoJobQueue>();
        }
    }
}
