using System.Collections.Generic;

namespace EventAggregatorAbstract.Fmaciasruano.Components
{
    public interface IEventAggregator
    {
        IEventUnsubscriber Subscribe(IEventSubscriptor subscriptor);
        List<IEventSubscriptor> GetEventSubscriptions(object triegger, string eventName);
        List<IProcessEvent> GetProcessEventSubscriptions(IProcessEvent processEvent);
        List<IEventSubscriptor> Subscriptions { get; }
        void UnsubscribeAll();
        IProcessEventFactory EventFactory { get; }
        IProcessEventSubscriptorFactory EventSubscriptorFactory { get; }
        IUIEventSubscriptorFactory UIEventSubscriptorFactory { get; }
    }
}
