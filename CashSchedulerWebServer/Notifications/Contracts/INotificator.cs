using System.Collections.Generic;
using System.Threading.Tasks;

namespace CashSchedulerWebServer.Notifications.Contracts
{
    public interface INotificator
    {
        Task SendEmail(string address, NotificationTemplateType type, Dictionary<string, string> parameters);
        Task SendEmail(string address, NotificationTemplate template);
    }
}
