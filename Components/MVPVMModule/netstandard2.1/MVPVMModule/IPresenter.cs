using EventAggregatorAbstract.Fmaciasruano.Components;

namespace fmacias.Components.MVPVMModule
{
    /// <summary>
    /// presenter
    /// </summary>
    public interface IPresenter<TViewType>
    {
        IBLL BLL { get; }
        IViewModel ViewModel { get; }
        IEventAggregator EventAggregator { get; }
        IPresenter<TViewType> SetView(TViewType view);
        void UnsubscribeAll();
    }
}
