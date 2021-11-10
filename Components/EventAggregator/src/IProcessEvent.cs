namespace fmacias.Components.EventAggregator
{
    public interface IProcessEvent
    {
        event ProcessEvent.ProcessEventHandler Event;

        void Publish();
    }
}