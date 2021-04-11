using System.Collections.Generic;
using CashSchedulerWebServer.Models;

namespace CashSchedulerWebServer.Db.Contracts
{
    public interface IUserNotificationRepository : IRepository<int, UserNotification>
    {
        IEnumerable<UserNotification> GetUnread();

        IEnumerable<UserNotification> DeleteByUserId(int userId);
    }
}
