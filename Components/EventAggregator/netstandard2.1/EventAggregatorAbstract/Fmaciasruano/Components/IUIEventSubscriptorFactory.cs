namespace EventAggregatorAbstract.Fmaciasruano.Components
{
    public interface IUIEventSubscriptorFactory
    {
        IUIEventSubscriptor Create(IEventAggregator eventAggregator);
    }
}
