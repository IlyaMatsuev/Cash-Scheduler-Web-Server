namespace CashSchedulerWebServer.Events.Contracts
{
    public interface IEventManager
    {
        void FireEvent(EventAction action, object entity);
    }
}
