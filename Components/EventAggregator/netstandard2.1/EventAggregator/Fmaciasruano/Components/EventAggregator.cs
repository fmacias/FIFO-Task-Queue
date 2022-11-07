using System.Collections.Generic;
using EventAggregatorAbstract.Fmaciasruano.Components;

namespace EventAggregator.Fmaciasruano.Components
{
    public class EventAggregator:IEventAggregator
    {
        private readonly IProcessEventFactory eventFactory;
        private readonly IProcessEventSubscriptorFactory eventSubscriptorFactory;
        private readonly IUIEventSubscriptorFactory uiEventSubscriptorFactory;
        List<IEventSubscriptor> eventSubscriptions = new List<IEventSubscriptor>();

        private EventAggregator(IProcessEventFactory eventFactory, IProcessEventSubscriptorFactory eventSubscriptorFactory, IUIEventSubscriptorFactory uiEventSubscriptorFactory)
        {
            this.eventFactory = eventFactory;
            this.eventSubscriptorFactory = eventSubscriptorFactory;
            this.uiEventSubscriptorFactory = uiEventSubscriptorFactory;
        }

        public static EventAggregator Create(IProcessEventFactory eventFactory, IProcessEventSubscriptorFactory processEventSubscriptorFactory, IUIEventSubscriptorFactory uiEventSubscriptorFactory)
        {
            return new EventAggregator(eventFactory,processEventSubscriptorFactory,uiEventSubscriptorFactory);
        }
        
        public List<IEventSubscriptor> Subscriptions => eventSubscriptions;

        public IProcessEventFactory EventFactory => eventFactory;

        public IProcessEventSubscriptorFactory EventSubscriptorFactory => eventSubscriptorFactory;

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
