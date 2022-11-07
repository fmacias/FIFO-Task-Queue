using EventAggregatorAbstract.Fmaciasruano.Components;

namespace EventAggregator.Fmaciasruano.Components
{
    public class UIEventSubscriptorFactory : IUIEventSubscriptorFactory
    {
        public static IUIEventSubscriptorFactory Instance { get; } = new UIEventSubscriptorFactory();

        public IUIEventSubscriptor Create(IEventAggregator eventAggregator)
        {
            return UIEventSubscriptor.Create(eventAggregator);
        }
    }
}
