namespace EventAggregatorAbstract.Fmaciasruano.Components
{
    public interface IUIEventSubscriptor:IEventSubscriptor
    {
        IEventSubscriptor AddEventHandler<TDelegate>(TDelegate handler, string eventName, object uiObject);
    }
}
