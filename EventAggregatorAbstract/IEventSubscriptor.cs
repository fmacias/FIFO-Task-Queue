using System;

namespace fmacias.Components.EventAggregator
{
    public interface IEventSubscriptor
    {
        IEventSubscriptor AddEventHandler<TDelegate>(TDelegate handler);
        void Unsubscribe();
        string EventName { get; }
        object TrieggerObject { get; }
        IEventUnsubscriber Unsubscriber { get; }
    }
}