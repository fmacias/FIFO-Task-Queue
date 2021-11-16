namespace fmacias.Components.EventAggregator
{
    public interface IProcessEvent
    {
        delegate void ProcessEventHandler(object sender);
        
        void Publish();
    }
}