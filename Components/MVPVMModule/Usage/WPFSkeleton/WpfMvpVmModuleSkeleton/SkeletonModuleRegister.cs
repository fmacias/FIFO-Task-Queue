using fmacias.Components.EventAggregator;
using fmacias.Components.FifoTaskQueueAbstract;
using fmacias.Components.MVPVMModule;
using fmacias.Components.MVPVMModule.Abstract;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using FifoTaskQueue;
using FifoTaskQueue.Fmaciasruano.Components;
using FifoTaskQueueAbstract;
using FifoTaskQueueAbstract.Fmaciasruano.Components;
using Unity;
using Unity.Injection;

namespace WpfMvpVmModuleSkeleton
{
    public class SkeletonModuleRegister : ModuleRegister<IUnityContainer>
    {
        public SkeletonModuleRegister(IUnityContainer iocContainer) : base(iocContainer)
        {
        }
        public override void Register<TViewType>()
        {
            base.Register<TViewType>();
        }
        protected override void RegisterBLL()
        {
            iocContainer.RegisterType<IBLL, SkeletonBLL>(
                GetUnicRegistrationName(typeof(SkeletonBLL)),
                new InjectionConstructor(ResolveConstructorObject<IDAL, SkeletonDAL>())
            );
        }
        protected override void RegisterDAL()
        {
            iocContainer.RegisterType<IDAL, SkeletonDAL>(
                GetUnicRegistrationName(typeof(SkeletonDAL)),
                new InjectionConstructor(
                    ResolveConstructorObject<IDomainModels, SkeletonDomainModels>(),
                    ResolveConstructorObject<IDomainModelFactory, DomainModelFactory>()
                )
            );
        }
        protected override void RegisterDomainModel()
        {
            iocContainer.RegisterType<IDomainModel, SkeletonDomainModel>(GetUnicRegistrationName(typeof(SkeletonDomainModel)));
        }
        protected override void RegisterDomainModelFactory()
        {
            iocContainer.RegisterType<IDomainModelFactory, DomainModelFactory>(GetUnicRegistrationName(typeof(DomainModelFactory)));
        }
        protected override void RegisterDomainModels()
        {
            iocContainer.RegisterType<IDomainModels, SkeletonDomainModels>(GetUnicRegistrationName(typeof(SkeletonDomainModels)));
        }
        protected override void RegisterEventAggregator()
        {
            iocContainer.RegisterType<IProcessEventFactory, ProcessEventFactory>(
                GetUnicRegistrationName(typeof(ProcessEventFactory))
            );

            iocContainer.RegisterType<IProcessEventSubscriptorFactory, ProcessEventSubscriptorFactory>(
                GetUnicRegistrationName(typeof(ProcessEventSubscriptorFactory))
            );

            iocContainer.RegisterType<IUIEventSubscriptorFactory, UIEventSubscriptorFactory>(
                GetUnicRegistrationName(typeof(UIEventSubscriptorFactory))
            );

            iocContainer.RegisterType<IEventAggregator, EventAggregator>(
                GetUnicRegistrationName(typeof(EventAggregator)),
                new InjectionConstructor(
                    ResolveConstructorObject<IProcessEventFactory, ProcessEventFactory>(),
                    ResolveConstructorObject<IProcessEventSubscriptorFactory, ProcessEventSubscriptorFactory>(),
                    ResolveConstructorObject<IUIEventSubscriptorFactory, UIEventSubscriptorFactory>()
                ));
        }
        protected override void RegisterExtras()
        {
            iocContainer.RegisterFactory<ILogger>((unityContainer) => LogManager.GetCurrentClassLogger());
        }
        protected override void RegisterFifoTaskQueue()
        {
            iocContainer.RegisterType<IGuiContextFifoTaskQueue, GuiContextFifoTaskQueue>(
                GetUnicRegistrationName(typeof(GuiContextFifoTaskQueue))
            );
            iocContainer.RegisterType<ICurrentContextFifoTaskQueue, CurrentContextFifoTaskQueue>(
                GetUnicRegistrationName(typeof(CurrentContextFifoTaskQueue))
            );
        }
        protected override void RegisterPresenter<TViewType>()
        {
            iocContainer.RegisterType<IPresenter<TViewType>, SkeletonPresenter<TViewType>>(
                GetUnicRegistrationName(typeof(SkeletonPresenter<TViewType>)),
                new InjectionConstructor(
                    ResolveConstructorObject<IBLL, SkeletonBLL>(),
                    ResolveConstructorObject<IViewModel, SkeletonViewModel>(),
                    ResolveConstructorObject<IEventAggregator, EventAggregator>(),
                    ResolveConstructorObject<IGuiContextFifoTaskQueue, GuiContextFifoTaskQueue>()
                )
            );
        }
        protected override void RegisterView<TViewType>()
        {
            iocContainer.RegisterType<TViewType>(
                new InjectionConstructor(ResolveConstructorObject<IPresenter<TViewType>,SkeletonPresenter<TViewType>>())
            );
        }
        protected override void RegisterViewModel()
        {
            iocContainer.RegisterType<IViewModel, SkeletonViewModel>(
                GetUnicRegistrationName(typeof(SkeletonViewModel))
            );
        }
        private object ResolveConstructorObject<TRegistered, TObject>()
        {
            return iocContainer.Resolve<TRegistered>(GetUnicRegistrationName(typeof(TObject)));
        }
    }
}
