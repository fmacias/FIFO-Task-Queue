using System;

namespace fmacias.Components.MVPVMModule.Abstract
{
    public abstract class ModuleRegister<TIOCCONTAINER> : IModuleRegister
    {
        protected readonly TIOCCONTAINER iocContainer;
        protected ModuleRegister(TIOCCONTAINER iocContainer)
        {
            this.iocContainer = iocContainer;
        }
        public virtual void Register<TViewType>()
        {
            RegisterExtras();
            RegisterFifoTaskQueue();
            RegisterEventAggregator();
            RegisterDomainModel();
            RegisterDomainModelFactory();
            RegisterDomainModels();
            RegisterViewModel();
            RegisterDAL();
            RegisterBLL();
            RegisterPresenter<TViewType>();
            RegisterView<TViewType>();
        }
        protected abstract void RegisterExtras();
        protected abstract void RegisterFifoTaskQueue();
        protected abstract void RegisterEventAggregator();
        protected abstract void RegisterDomainModel();
        protected abstract void RegisterDomainModelFactory();
        protected abstract void RegisterDomainModels();
        protected abstract void RegisterViewModel();
        protected abstract void RegisterDAL();
        protected abstract void RegisterBLL();
        protected abstract void RegisterPresenter<TViewType>();
        protected abstract void RegisterView<TViewType>();
        public string GetUnicRegistrationName(Type type)
        {
            return string.Format("{0}.{1}", GetType().FullName, type.FullName);
        }
    }
}
