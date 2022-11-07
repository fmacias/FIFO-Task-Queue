namespace EventAggregatorAbstract.Fmaciasruano.Components
{
    public interface IEventSubscriptor
    {
        void Unsubscribe();
        string EventName { get; }
        object TrieggerObject { get; }
        IEventUnsubscriber Unsubscriber { get; }
    }
}