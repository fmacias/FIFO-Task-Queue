using fmacias;
using fmacias.Components.FifoTaskQueue;
using NLog;
using System;
using System.Collections.Generic;
using System.Text;
using Unity;
using WPF_Usage.ExampleModule.BLL;
using WPF_Usage.ExampleModule.Model;
using WPF_Usage.ExampleModule.Presenter;
using WPF_Usage.ExampleModule.ViewModel;

namespace WPF_Usage.ExampleModule
{
    public class Module: IModuleRegister
    {
        [InjectionMethod]
        public void Register(IUnityContainer container)
        {
            RegisterExampleView(container);
        }
        #region private
        private void RegisterExampleView(IUnityContainer container)
        {
            RegisterExamplePresenterDependencies(container);
            container.RegisterType<IExamplePresenter, ExamplePresenter>();
        }
        private void RegisterExamplePresenterDependencies(IUnityContainer container)
        {
            RegisterExampleBLLDependencies(container);
            RegisterExampleViewModelDependencies(container);
            container.RegisterType<IExampleBLL, ExampleBLL>();
            container.RegisterType<IExampleViewModel, ExampleViewModel>();
        }
        private void RegisterExampleBLLDependencies(IUnityContainer container)
        {
            container.RegisterFactory<ITaskQueue>((unityContainer) => (ITaskQueue)FifoTaskQueue.Create(TaskShedulerWraper.Create().FromCurrentWorker(), LogManager.GetCurrentClassLogger()));
        }
        private void RegisterExampleViewModelDependencies(IUnityContainer container)
        {
            container.RegisterType<IExampleModel, ExampleModel>();
            container.RegisterFactory<IExampleModel>((unityContainer) => unityContainer.Resolve<ExampleModel>());
        }
        #endregion
    }
}
