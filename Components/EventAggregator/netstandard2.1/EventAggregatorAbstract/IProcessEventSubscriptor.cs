using System;
using System.Collections.Generic;
using System.Text;

namespace fmacias.Components.EventAggregator
{
    public interface IProcessEventSubscriptor: IEventSubscriptor
    {
        IEventSubscriptor AddEventHandler<TDelegate>(TDelegate handler, IProcessEvent processEvent);
    }
}
