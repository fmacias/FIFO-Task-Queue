using System;
using System.Collections.Generic;
using System.Text;
using EventAggregatorAbstract.Fmaciasruano.Components;

namespace FifoTaskQueue.Fmaciasruano.Components
{
    public static class SubscriptorHelper
    {
        public static void AddUnicEventHandler(IProcessEvent.ProcessEventHandler handler, IProcessEventSubscriptor processEventSubscriptor, 
            IEventAggregator eventAggregator, object objectSubscriptor)
        {
            var callbackProcessEvent =
                (ICallbackProcessEvent)eventAggregator.EventFactory.Create<CallbackProcessEvent>();
            callbackProcessEvent.Sender = objectSubscriptor;
            processEventSubscriptor.AddEventHandler(handler, callbackProcessEvent);
        }
    }
}
