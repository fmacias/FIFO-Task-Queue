using EventAggregatorAbstract.Fmaciasruano.Components;

namespace EventAggregator.Fmaciasruano.Components
{
    public class ProcessEventSubscriptor: EventSubscriptorAbstract, IProcessEventSubscriptor
    {
        /// <summary>
        /// Default event name of IProccessEvent Object
        /// </summary>
        private new const string EventName = "Event";

        private ProcessEventSubscriptor(IEventAggregator eventAggregator) : base(eventAggregator)
        {
            this.eventName = EventName;
        }

        public static ProcessEventSubscriptor Create(IEventAggregator eventAggregator)
        {
            return new ProcessEventSubscriptor(eventAggregator);
        } 
        
        public void Publish()
        {
            IProcessEvent processEvent = (IProcessEvent) trieggerEventSource;
            processEvent.Publish();
        }

        IEventSubscriptor IProcessEventSubscriptor.AddEventHandler<TDelegate>(TDelegate handler, IProcessEvent processEvent)
        {
            this.trieggerEventSource = processEvent;
            this.AddEventHandler<TDelegate>(handler);
            return this;
        }
    }
}
