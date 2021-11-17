using System;
using System.Collections.Generic;
using System.Text;

namespace fmacias.Components.EventAggregator
{
    public class ProcessEventSubscriptorFactory : IProcessEventSubscriptorFactory
    {
        public IProcessEventSubscriptor Create(IEventSubscriptable eventSubscriptable)
        {
            return new ProcessEventSubscriptor(eventSubscriptable);
        }
    }
}
