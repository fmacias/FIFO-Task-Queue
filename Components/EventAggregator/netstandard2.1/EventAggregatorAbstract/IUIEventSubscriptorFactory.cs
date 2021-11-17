using System;
using System.Collections.Generic;
using System.Text;

namespace fmacias.Components.EventAggregator
{
    public interface IUIEventSubscriptorFactory
    {
        IUIEventSubscriptor Create(IEventSubscriptable eventSubscriptable);
    }
}
