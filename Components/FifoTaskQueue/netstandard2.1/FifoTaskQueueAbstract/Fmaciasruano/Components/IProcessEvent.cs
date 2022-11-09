namespace FifoTaskQueueAbstract.Fmaciasruano.Components
{
    public interface IProcessEvent
    {
        delegate void ProcessEventHandler(object sender);
        void Publish();
        IProcessEvent AddEventHandler<TDelegate>(TDelegate handler);
        void RemoveEventHandler();
    }
}