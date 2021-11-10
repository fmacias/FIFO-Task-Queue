using System;
using System.Collections.Generic;
using System.Linq;
namespace fmacias.Components.EventAggregator
{
    public class EventAggregator:IEventSubscriptable
    {

        List<IEventSubscriptor> eventSubscriptions = new List<IEventSubscriptor>();
        public List<IEventSubscriptor> Subscriptions => eventSubscriptions;
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
