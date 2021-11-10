using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Text;

namespace fmacias.Components.EventAggregator
{
    public class ProcessEvent : IProcessEvent
    {
        public delegate void ProcessEventHandler(object sender);
        public event ProcessEventHandler Event;
        public ISynchronizeInvoke SynchronizingObject { get; set; }
        public void Publish()
        {
            // Event not subscribed
            if (Event == null)
                return;
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.

            ProcessEventHandler raiseEvent = Event;
            // Event will be null if there are no subscribers
            if (raiseEvent != null)
            {
                // Call to raise the event.
                raiseEvent(this);
            }
        }
    }
}
