namespace FifoTaskQueueAbstract.Fmaciasruano.Components
{
    public enum ObserverStatus
    {
        Created = 0,
        Observing = 1,
        Completed = 2,
        CompletedWithErrors = 3,
        ExecutionTimeExceeded = 4,
        Canceled = 5,
        Unknown = 6
    }
}
