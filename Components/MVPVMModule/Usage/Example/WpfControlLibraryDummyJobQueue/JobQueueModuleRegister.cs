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

namespace WpfControlLibraryDummyJobQueue
{
    public class JobQueueModuleRegister : ModuleRegister<IUnityContainer>, IJobQueueModuleRegister
    {
        public JobQueueModuleRegister(IUnityContainer iocContainer) : base(iocContainer)
        {
        }
        public override void Register<TViewType>()
        {
            base.Register<TViewType>();
        }
        protected override void RegisterBLL()
        {
            iocContainer.RegisterType<IBLL, JobQueueBLL>(
                GetUnicRegistrationName(typeof(JobQueueBLL)),
                new InjectionConstructor(ResolveConstructorObject<IDAL, JobQueueDAL>())
            );
        }
        protected override void RegisterDAL()
        {
            iocContainer.RegisterType<IDAL, JobQueueDAL>(
                GetUnicRegistrationName(typeof(JobQueueDAL)),
                new InjectionConstructor(
                    ResolveConstructorObject<IDomainModels, JobQueueDomainModels>(),
                    ResolveConstructorObject<IDomainModelFactory, DomainModelFactory>()
                )
            );
        }
        protected override void RegisterDomainModel()
        {
            iocContainer.RegisterType<IDomainModel, JobQueueDomainModel>(GetUnicRegistrationName(typeof(JobQueueDomainModel)));
        }
        protected override void RegisterDomainModelFactory()
        {
            iocContainer.RegisterType<IDomainModelFactory, DomainModelFactory>(GetUnicRegistrationName(typeof(DomainModelFactory)));
        }
        protected override void RegisterDomainModels()
        {
            iocContainer.RegisterType<IDomainModels, JobQueueDomainModels>(GetUnicRegistrationName(typeof(JobQueueDomainModels)));
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
            iocContainer.RegisterType<IPresenter<TViewType>, JobQueuePresenter<TViewType>>(
                GetUnicRegistrationName(typeof(JobQueuePresenter<TViewType>)),
                new InjectionConstructor(
                    ResolveConstructorObject<IBLL, JobQueueBLL>(),
                    ResolveConstructorObject<IViewModel, JobQueueViewModel>(),
                    ResolveConstructorObject<IEventAggregator, EventAggregator>(),
                    ResolveConstructorObject<IGuiContextFifoTaskQueue, GuiContextFifoTaskQueue>()
                )
            );
        }
        protected override void RegisterView<TViewType>()
        {
            iocContainer.RegisterType<TViewType>(
                new InjectionConstructor(ResolveConstructorObject<IPresenter<TViewType>,JobQueuePresenter<TViewType>>())
            );
        }
        protected override void RegisterViewModel()
        {
            iocContainer.RegisterType<IViewModel, JobQueueViewModel>(
                GetUnicRegistrationName(typeof(JobQueueViewModel))
            );
        }
        private object ResolveConstructorObject<TRegistered, TObject>()
        {
            return iocContainer.Resolve<TRegistered>(GetUnicRegistrationName(typeof(TObject)));
        }
    }
}
