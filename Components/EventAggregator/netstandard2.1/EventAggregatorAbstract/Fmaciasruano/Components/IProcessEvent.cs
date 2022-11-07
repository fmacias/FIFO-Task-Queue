namespace EventAggregatorAbstract.Fmaciasruano.Components
{
    public interface IProcessEvent: IEventPublisher
    {
        delegate void ProcessEventHandler(object sender);
    }
}