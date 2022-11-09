using EventAggregatorAbstract.Fmaciasruano.Components;

namespace fmacias.Components.MVPVMModule
{
    public abstract class Presenter<TViewType> : IPresenter<TViewType>
    {
        protected readonly IBLL bll;
        protected readonly IViewModel viewModel;
        protected readonly IEventAggregator eventAggregator;
        protected TViewType view;

        protected Presenter(IBLL bll, IViewModel viewModel, IEventAggregator eventAggregator)
        {
            this.bll = bll;
            this.viewModel = viewModel;
            this.eventAggregator = eventAggregator;
        }
        public IBLL BLL => bll;

        public IViewModel ViewModel => viewModel;

        public IEventAggregator EventAggregator => eventAggregator;

		IEventAggregator IPresenter<TViewType>.EventAggregator => throw new System.NotImplementedException();

		public virtual IPresenter<TViewType> SetView(TViewType view)
        {
            this.view = view;
            return this;
        }

        public virtual void UnsubscribeAll()
        {
            this.eventAggregator.UnsubscribeAll();
        }
    }
}
