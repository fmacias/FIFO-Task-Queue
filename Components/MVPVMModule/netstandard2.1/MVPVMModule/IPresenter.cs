using fmacias.Components.EventAggregator;

namespace fmacias.Components.MVPVMModule
{
    /// <summary>
    /// presenter
    /// </summary>
    public interface IPresenter<TViewType>
    {
        IBLL BLL { get; }
        IViewModel ViewModel { get; }
        IEventSubscriptable EventAggregator { get; }
        IPresenter<TViewType> SetView(TViewType view);
        void UnsubscribeAll();
    }
}
