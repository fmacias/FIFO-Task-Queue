using System;
using System.Collections.Generic;
using System.Text;

namespace fmacias.Components.EventAggregator
{
    public class UIEventSubscriptor : EventSubscriptorAbstract, IUIEventSubscriptor
    {
        public UIEventSubscriptor(IEventSubscriptable eventAggregator) : base(eventAggregator) { }
        public IEventSubscriptor AddEventHandler<TDelegate>(TDelegate handler, string eventName, object uiObject)
        {
            this.eventName = eventName;
            this.trieggerEventSource = uiObject;
            this.AddEventHandler<TDelegate>(handler);
            return this;
        }
    }
}
