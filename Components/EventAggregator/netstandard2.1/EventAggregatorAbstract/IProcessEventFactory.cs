namespace fmacias.Components.EventAggregator
{
    public interface IProcessEventFactory
    {
        IProcessEvent Create<T>() where T : new();
    }
}