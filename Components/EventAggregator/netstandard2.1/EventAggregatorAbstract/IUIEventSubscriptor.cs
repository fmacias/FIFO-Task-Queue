using System;
using System.Collections.Generic;
using System.Text;

namespace fmacias.Components.EventAggregator
{
    public interface IUIEventSubscriptor:IEventSubscriptor
    {
        IEventSubscriptor AddEventHandler<TDelegate>(TDelegate handler, string eventName, object uiObject);
    }
}
