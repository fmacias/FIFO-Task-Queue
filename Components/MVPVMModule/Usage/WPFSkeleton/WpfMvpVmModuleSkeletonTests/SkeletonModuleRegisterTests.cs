using NUnit.Framework;
using WpfMvpVmModuleSkeleton;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;
using fmacias.Components.EventAggregator;
using fmacias.Components.FifoTaskQueueAbstract;
using fmacias.Components.FifoTaskQueue;
using System.Threading;
using fmacias.Components.MVPVMModule;

namespace WpfMvpVmModuleSkeleton.Tests
{
    [TestFixture()]
    public class SkeletonModuleRegisterTests
    {
        private SkeletonModuleRegister register;
        [SetUp]
        public void TestSetUp()
        {
            SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
        }
        private string GetRegistrationName(Type type)
        {
            return register.GetUnicRegistrationName(type);
        }
        [Test()]
        public void RegisterTest()
        {
            using ( IUnityContainer container = new UnityContainer())
            {
                register = new SkeletonModuleRegister(container);
                register.Register<SkeletonView>();
                Assert.IsInstanceOf<ProcessEventFactory>(container.Resolve<IProcessEventFactory>(GetRegistrationName(typeof(ProcessEventFactory))));
                Assert.IsInstanceOf<EventAggregator>(container.Resolve<IEventSubscriptable>(GetRegistrationName(typeof(EventAggregator))));
                var processEventSubscriptorFactory = container.Resolve<IProcessEventSubscriptorFactory>(GetRegistrationName(typeof(ProcessEventSubscriptorFactory)));
                Assert.IsInstanceOf<ProcessEventSubscriptorFactory>(processEventSubscriptorFactory);
                var uiEventSubscriptorFatory = container.Resolve<IUIEventSubscriptorFactory>(GetRegistrationName(typeof(UIEventSubscriptorFactory)));
                Assert.IsInstanceOf<UIEventSubscriptorFactory>(uiEventSubscriptorFatory);
                Assert.IsInstanceOf<CurrentContextFifoTaskQueue>(container.Resolve<ICurrentContextFifoTaskQueue>(GetRegistrationName(typeof(CurrentContextFifoTaskQueue))));
                Assert.IsInstanceOf<GuiContextFifoTaskQueue>(container.Resolve<IGuiContextFifoTaskQueue>(GetRegistrationName(typeof(GuiContextFifoTaskQueue))));
                Assert.IsInstanceOf<SkeletonDomainModel>(container.Resolve<IDomainModel>(GetRegistrationName(typeof(SkeletonDomainModel))));
                Assert.IsInstanceOf<DomainModelFactory>(container.Resolve<IDomainModelFactory>(GetRegistrationName(typeof(DomainModelFactory))));
                Assert.IsInstanceOf<SkeletonDomainModels>(container.Resolve<IDomainModels>(GetRegistrationName(typeof(SkeletonDomainModels))));
                Assert.IsInstanceOf<SkeletonViewModel>(container.Resolve<IViewModel>(GetRegistrationName(typeof(SkeletonViewModel))));
                Assert.IsInstanceOf<SkeletonDAL>(container.Resolve<IDAL>(GetRegistrationName(typeof(SkeletonDAL))));
                Assert.IsInstanceOf<SkeletonBLL>(container.Resolve<IBLL>(GetRegistrationName(typeof(SkeletonBLL))));
                Assert.IsInstanceOf<SkeletonPresenter<SkeletonView>>(container.Resolve<IPresenter<SkeletonView>>(GetRegistrationName(typeof(SkeletonPresenter<SkeletonView>))));
            }
        }
        [Test]
        [Apartment(ApartmentState.STA)]
        public void ResolveExampleViewTest()
        {
            using (IUnityContainer container = new UnityContainer())
            {
                SkeletonModuleRegister register = new SkeletonModuleRegister(container);
                register.Register<SkeletonView>();
                SkeletonView view = container.Resolve<SkeletonView>();
                Assert.IsInstanceOf<SkeletonView>(view);
                Assert.IsInstanceOf<SkeletonPresenter<SkeletonView>>(view.Presenter);
            }
        }
    }
}