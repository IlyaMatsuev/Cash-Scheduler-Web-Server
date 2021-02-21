using System.Threading.Tasks;

namespace CashSchedulerWebServer.Events.Contracts
{
    public interface IEventManager
    {
        Task FireEvent(EventAction action, object entity);
    }
}
