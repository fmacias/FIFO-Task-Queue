using System;
using System.Collections.Generic;
using System.Linq;
namespace fmacias.Components.EventAggregator
{
    public class EventAggregator:IEventSubscriptable
    {
        private readonly IProcessEventFactory processEventFactory;
        private readonly IProcessEventSubscriptorFactory processEventSubscriptorFactory;
        private readonly IUIEventSubscriptorFactory uiEventSubscriptorFactory;

        List<IEventSubscriptor> eventSubscriptions = new List<IEventSubscriptor>();
        public EventAggregator(IProcessEventFactory processEventFactory, IProcessEventSubscriptorFactory processEventSubscriptorFactory, IUIEventSubscriptorFactory uiEventSubscriptorFactory)
        {
            this.processEventFactory = processEventFactory;
            this.processEventSubscriptorFactory = processEventSubscriptorFactory;
            this.uiEventSubscriptorFactory = uiEventSubscriptorFactory;
        }

        public List<IEventSubscriptor> Subscriptions => eventSubscriptions;

        public IProcessEventFactory ProcessEventFactory => processEventFactory;

        public IProcessEventSubscriptorFactory ProcessEventSubscriptorFactory => processEventSubscriptorFactory;

        public IUIEventSubscriptorFactory UIEventSubscriptorFactory => uiEventSubscriptorFactory;

        public List<IEventSubscriptor> GetEventSubscriptions(object triegger, string eventName)
        {
            return this.eventSubscriptions.FindAll(
                subscription => object.ReferenceEquals(subscription.TrieggerObject, triegger) &&
                subscription.EventName == eventName
            );
        }
        public List<IProcessEvent> GetProcessEventSubscriptions(IProcessEvent processEvent)
        {
            List<IProcessEvent> processEventSubscriptions = new List<IProcessEvent>();
            this.eventSubscriptions.FindAll(
                subscription => object.ReferenceEquals(subscription.TrieggerObject, processEvent)
            ).ForEach(s => processEventSubscriptions.Add((IProcessEvent)s.TrieggerObject));
            return processEventSubscriptions;
        }
        public IEventUnsubscriber Subscribe(IEventSubscriptor subscriptor)
        {
            if (eventSubscriptions.Contains(subscriptor))
                return subscriptor.Unsubscriber;
            
            eventSubscriptions.Add(subscriptor);
            return EventUnsubscriber<IEventSubscriptor>.Create(eventSubscriptions, subscriptor);
        }

        public void UnsubscribeAll()
        {
            List<IEventSubscriptor> subscriptions= new List<IEventSubscriptor>();
            eventSubscriptions.ForEach(s => subscriptions.Add(s));
            subscriptions.ForEach(s => s.Unsubscribe());
        }
    }
}
