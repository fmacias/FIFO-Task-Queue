using NUnit.Framework;
using WPF_Usage.ExampleModule;
using System;
using System.Collections.Generic;
using System.Text;
using Unity;
using WPF_Usage.ExampleModule.BLL;
using fmacias;
using System.Threading;
using WPF_Usage.ExampleModule.ViewModel;
using WPF_Usage.ExampleModule.Model;
using System.ComponentModel;
using WPF_Usage.ExampleModule.Presenter;

namespace WPF_Usage.ExampleModule.Tests
{
    [TestFixture()]
    public class ModuleTests
    {
        [Test()]
        public void RegisterTest()
        {
            using (IUnityContainer container = new UnityContainer())
            {
                container.RegisterType<ExampleModule.Module>();
                Assert.IsInstanceOf<ExampleModule.Module>(container.Resolve<ExampleModule.Module>());
            }
        }
        [Test]
        public void ResolveTaskQueueFactoryTest()
        {
            using (IUnityContainer container = new UnityContainer())
            {
                container.Resolve<ExampleModule.Module>();
                ExampleBLL webContentBLL = container.Resolve<ExampleBLL>();
                Assert.IsInstanceOf<ITaskQueue>(webContentBLL.TaskQueueFactory.Invoke());
            }
        }
        [Test]
        public void ResolveExampleViewModelTest()
        {
            using (IUnityContainer container = new UnityContainer())
            {
                container.Resolve<ExampleModule.Module>();
                ExampleViewModel viewModel = container.Resolve<ExampleViewModel>();
                Assert.IsInstanceOf<IExampleModel>(viewModel.webContentModel.Invoke());
                Assert.IsInstanceOf<INotifyPropertyChanged>(viewModel);
            }
        }
        [Test]
        public void ResolveExamplePresenterTest()
        {
            using (IUnityContainer container = new UnityContainer())
            {
                container.Resolve<ExampleModule.Module>();
                Assert.IsInstanceOf<ExamplePresenter>(container.Resolve<ExamplePresenter>());
            }
        }
        [Test]
        [Apartment(ApartmentState.STA)]
        public void ResolveExampleViewTest()
        {
            using (IUnityContainer container = new UnityContainer())
            {
                container.Resolve<ExampleModule.Module>();
                ExampleView view = container.Resolve<ExampleView>();
                Assert.IsInstanceOf<ExamplePresenter>(view.ExamplePresenter);
                ExampleBLL webContentBLL = container.Resolve<ExampleBLL>();
                Assert.IsInstanceOf<ITaskQueue>(webContentBLL.TaskQueueFactory.Invoke());
            }
        }
    }
}