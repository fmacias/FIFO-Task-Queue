using EventAggregatorAbstract.Fmaciasruano.Components;

namespace FifoTaskQueue.Fmaciasruano.Components
{
    internal class CallbackProcessEvent: ICallbackProcessEvent
    {
        public event IProcessEvent.ProcessEventHandler Event;
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

        public object Sender { get; set; }
    }
}
