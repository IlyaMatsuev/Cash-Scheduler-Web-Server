using CashSchedulerWebServer.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CashSchedulerWebServer.Db.Contracts
{
    public interface IUserNotificationRepository : IRepository<int, UserNotification>
    {
        IEnumerable<UserNotification> GetAllUnread();
        Task<UserNotification> Read(int id);
        Task<UserNotification> Unread(int id);
    }
}
