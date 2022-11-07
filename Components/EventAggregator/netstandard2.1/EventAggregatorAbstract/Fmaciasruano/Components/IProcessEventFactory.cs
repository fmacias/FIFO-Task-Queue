namespace EventAggregatorAbstract.Fmaciasruano.Components
{
    public interface IProcessEventFactory
    {
        IProcessEvent Create<T>() where T : new();
    }
}