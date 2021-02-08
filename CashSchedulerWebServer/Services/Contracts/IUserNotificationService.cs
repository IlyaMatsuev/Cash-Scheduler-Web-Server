using System.Collections.Generic;
using System.Threading.Tasks;
using CashSchedulerWebServer.Models;

namespace CashSchedulerWebServer.Services.Contracts
{
    public interface IUserNotificationService : IService<int, UserNotification>
    {
        IEnumerable<UserNotification> GetAll();
        Task<UserNotification> ToggleRead(int id, bool read);
    }
}
