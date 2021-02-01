using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Models;
using CashSchedulerWebServer.Utils;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CashSchedulerWebServer.Auth.Contracts;
using CashSchedulerWebServer.Exceptions;

namespace CashSchedulerWebServer.Db.Repositories
{
    public class UserNotificationRepository : IUserNotificationRepository
    {
        private CashSchedulerContext Context { get; }
        private IContextProvider ContextProvider { get; }
        private int UserId { get; }

        public UserNotificationRepository(
            CashSchedulerContext context,
            IUserContext userContext,
            IContextProvider contextProvider)
        {
            Context = context;
            ContextProvider = contextProvider;
            UserId = userContext.GetUserId();
        }
        

        public IEnumerable<UserNotification> GetAll()
        {
            return Context.UserNotifications.Where(n => n.User.Id == UserId).Include(n => n.User);
        }

        public IEnumerable<UserNotification> GetAllUnread()
        {
            return Context.UserNotifications.Where(n => n.User.Id == UserId && !n.IsRead)
                .Include(n => n.User);
        }

        public UserNotification GetById(int id)
        {
            return Context.UserNotifications.Where(n => n.Id == id && n.User.Id == UserId)
                .Include(n => n.User)
                .FirstOrDefault();
        }

        public async Task<UserNotification> Create(UserNotification notification)
        {
            notification.User ??= ContextProvider.GetRepository<IUserRepository>().GetById(UserId);

            ModelValidator.ValidateModelAttributes(notification);

            Context.UserNotifications.Add(notification);
            await Context.SaveChangesAsync();

            return GetById(notification.Id);
        }

        public async Task<UserNotification> Update(UserNotification notification)
        {
            var targetNotification = GetById(notification.Id);
            if (targetNotification == null)
            {
                throw new CashSchedulerException("There is no such notification");
            }

            if (notification.Title != default)
            {
                targetNotification.Title = notification.Title;
            }

            if (notification.Content != default)
            {
                targetNotification.Content = notification.Content;
            }

            targetNotification.IsRead = notification.IsRead;

            ModelValidator.ValidateModelAttributes(targetNotification);

            Context.UserNotifications.Update(targetNotification);
            await Context.SaveChangesAsync();

            return targetNotification;
        }

        public Task<UserNotification> Read(int id)
        {
            return Update(new UserNotification
            {
                Id = id,
                IsRead = true
            });
        }

        public Task<UserNotification> Unread(int id)
        {
            return Update(new UserNotification
            {
                Id = id,
                IsRead = false
            });
        }

        public async Task<UserNotification> Delete(int notificationId)
        {
            var targetNotification = GetById(notificationId);
            if (targetNotification == null)
            {
                throw new CashSchedulerException("There is no such notification");
            }

            Context.UserNotifications.Remove(targetNotification);
            await Context.SaveChangesAsync();

            return targetNotification;
        }
    }
}
