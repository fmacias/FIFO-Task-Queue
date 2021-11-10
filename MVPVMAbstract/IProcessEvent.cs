namespace MVPVMAbstract
{
    public interface IProcessEvent
    {
        event ProcessEvent.ProcessEventHandler Event;

        void Publish();
    }
}