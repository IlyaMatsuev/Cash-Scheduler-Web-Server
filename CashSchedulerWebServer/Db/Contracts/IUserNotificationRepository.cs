using CashSchedulerWebServer.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CashSchedulerWebServer.Db.Contracts
{
    interface IUserNotificationRepository : IRepository<UserNotification>
    {
        IEnumerable<UserNotification> GetAllUnread();
        Task<UserNotification> Read(int id);
        Task<UserNotification> Unread(int id);
    }
}
