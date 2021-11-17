using System;
using System.Collections.Generic;
using System.Text;

namespace fmacias.Components.EventAggregator
{
    public class UIEventSubscriptorFactory : IUIEventSubscriptorFactory
    {
        public IUIEventSubscriptor Create(IEventSubscriptable eventSubscriptable)
        {
            return new UIEventSubscriptor(eventSubscriptable);
        }
    }
}
