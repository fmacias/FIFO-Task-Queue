using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace fmacias.Components.EventAggregator
{
    public class ProcessEventSubscriptor: EventSubscriptorAbstract, IProcessEventSubscriptor
    {
        /// <summary>
        /// Default event name of IProccessEvent Object
        /// </summary>
        private const string EVENT_NAME = "Event";
        public ProcessEventSubscriptor(IEventSubscriptable eventAggregator) : base(eventAggregator)
        {           
            this.eventName = EVENT_NAME;
        }

        IEventSubscriptor IProcessEventSubscriptor.AddEventHandler<TDelegate>(TDelegate handler, IProcessEvent processEvent)
        {
            this.trieggerEventSource = processEvent;
            this.AddEventHandler<TDelegate>(handler);
            return this;
        }
    }
}
