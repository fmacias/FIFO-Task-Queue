using System;
using System.Collections.Generic;
using System.Text;

namespace fmacias.Components.EventAggregator
{
    public interface IProcessEventSubscriptorFactory
    {
        IProcessEventSubscriptor Create(IEventSubscriptable eventSubscriptable);
    }
}
