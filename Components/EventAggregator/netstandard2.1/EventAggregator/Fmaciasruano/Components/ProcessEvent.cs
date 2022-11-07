using System.Threading;
using System.Threading.Tasks;
using EventAggregatorAbstract.Fmaciasruano.Components;

namespace EventAggregator.Fmaciasruano.Components
{
    public class ProcessEvent : IProcessEvent
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
            raiseEvent?.Invoke(this);
        }
    }
}
