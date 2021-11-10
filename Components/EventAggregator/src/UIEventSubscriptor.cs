using System;
using System.Collections.Generic;
using System.Text;

namespace fmacias.Components.EventAggregator
{
    public class UIEventSubscriptor : EventSubscriptorAbstract
    {
        public UIEventSubscriptor(IEventSubscriptable eventAggregator, object uiObject, string eventName):base(eventAggregator)
        {
            if (uiObject.GetType().GetEvent(eventName) == null)
                throw new MVPVMAbstractException(
                    string.Format("Object Type {0} is not subscriptable for event {1}.", typeof(IProcessEvent).ToString(), "Event")
                );
            this.triegger = uiObject;
            this.eventName = eventName;
        }
    }
}
