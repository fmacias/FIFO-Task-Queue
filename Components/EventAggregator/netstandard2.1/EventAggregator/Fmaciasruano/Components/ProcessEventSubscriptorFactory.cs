using EventAggregatorAbstract.Fmaciasruano.Components;

namespace EventAggregator.Fmaciasruano.Components
{
    public class ProcessEventSubscriptorFactory : IProcessEventSubscriptorFactory
    {
        private ProcessEventSubscriptorFactory()
        {
        }

        public static IProcessEventSubscriptorFactory Instance { get; } = new ProcessEventSubscriptorFactory();

        public IProcessEventSubscriptor Create(IEventAggregator eventAggregator)
        {
            return ProcessEventSubscriptor.Create(eventAggregator);
        }
    }
}
