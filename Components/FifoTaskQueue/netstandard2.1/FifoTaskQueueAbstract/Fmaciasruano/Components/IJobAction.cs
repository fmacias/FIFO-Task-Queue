namespace FifoTaskQueueAbstract.Fmaciasruano.Components
{
    public interface IJobAction<TAction>: IJob<TAction>
    {
        IJob<TAction> Set(TAction action);
    }
}