namespace EventAggregatorAbstract.Fmaciasruano.Components
{
    public interface IProcessEventSubscriptor: IEventSubscriptor, IEventPublisher
    {
        IEventSubscriptor AddEventHandler<TDelegate>(TDelegate handler, IProcessEvent processEvent);
    }
}
