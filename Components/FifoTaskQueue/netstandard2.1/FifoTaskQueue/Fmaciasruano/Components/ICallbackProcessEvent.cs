using EventAggregatorAbstract.Fmaciasruano.Components;

namespace FifoTaskQueue.Fmaciasruano.Components
{
    internal interface ICallbackProcessEvent: IProcessEvent
    {
        object Sender { get; set; }
    }
}
