using FifoTaskQueueAbstract.Fmaciasruano.Components;
using System;
using System.Reflection;

namespace FifoTaskQueue.Fmaciasruano.Components
{
    internal class CallbackProcessEvent: ICallbackProcessEvent
    {
        public event IProcessEvent.ProcessEventHandler Event;
        EventInfo eventInfo;
        protected Delegate handler;
        private CallbackProcessEvent() { }

        public static ICallbackProcessEvent Create()
        {
            return new CallbackProcessEvent();
        }
        public void Publish()
        {
            // Event not subscribed
            // Make a temporary copy of the event to avoid possibility of
            // a race condition if the last subscriber unsubscribes
            // immediately after the null check and before the event is raised.

            IProcessEvent.ProcessEventHandler raiseEvent = Event;
            // Event will be null if there are no subscribers
            // Call to raise the event.
            object sender = Sender ?? this;
            raiseEvent?.Invoke(sender);
        }

		public IProcessEvent AddEventHandler<TDelegate>(TDelegate handler)
		{
            if (eventInfo == null)
            {
                eventInfo = this.GetType().GetEvent("Event");
                this.handler = handler as Delegate;
                eventInfo.AddEventHandler(this, this.handler);
            }
            return this;
        }

		public void RemoveEventHandler()
		{
            if (!(eventInfo == null))
                eventInfo.RemoveEventHandler(this, this.handler);
        }

		public object Sender { get; set; }
    }
}
