using System.Threading.Tasks;

namespace CashSchedulerWebServer.Events.Contracts
{
    public interface IEventListener
    {
        EventAction Action { get; }

        Task Handle(object entity);
    }
}
