namespace EventAggregatorAbstract.Fmaciasruano.Components
{
    public interface IProcessEventSubscriptorFactory
    {
        IProcessEventSubscriptor Create(IEventAggregator eventAggregator);
    }
}
