using System;

namespace MVPVMAbstract
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