using System;
using System.Collections.Generic;
using System.Text;

namespace fmacias.Components.EventAggregator
{
    public interface IEventAggregator<T>: IObservable<T>
    {
        void Publish<T>(T EventData);
    }
}
