using System;

namespace fmacias.Components.EventAggregator
{
    public interface IEventSubscriptor
    {
        void Unsubscribe();
        string EventName { get; }
        object TrieggerObject { get; }
        IEventUnsubscriber Unsubscriber { get; }
    }
}