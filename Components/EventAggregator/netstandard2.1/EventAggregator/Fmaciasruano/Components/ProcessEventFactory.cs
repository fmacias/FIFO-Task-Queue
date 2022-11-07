using EventAggregatorAbstract.Fmaciasruano.Components;

namespace EventAggregator.Fmaciasruano.Components
{
    public class ProcessEventFactory : IProcessEventFactory
    {
        private ProcessEventFactory()
        {
        }

        public static IProcessEventFactory Instance { get; } = new ProcessEventFactory();

        public IProcessEvent Create<T>() where T : new()
        {
            return (IProcessEvent) new T();
        }
    }
}
