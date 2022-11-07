using EventAggregatorAbstract.Fmaciasruano.Components;

namespace EventAggregator.Fmaciasruano.Components
{
    public class UIEventSubscriptor : EventSubscriptorAbstract, IUIEventSubscriptor
    {
        private UIEventSubscriptor(IEventAggregator eventAggregator) : base(eventAggregator) { }

        public static UIEventSubscriptor Create(IEventAggregator eventAggregator)
        {
            return new UIEventSubscriptor(eventAggregator);
        }

        public IEventSubscriptor AddEventHandler<TDelegate>(TDelegate handler, string eventName, object uiObject)
        {
            this.eventName = eventName;
            this.trieggerEventSource = uiObject;
            this.AddEventHandler<TDelegate>(handler);
            return this;
        }
    }
}
