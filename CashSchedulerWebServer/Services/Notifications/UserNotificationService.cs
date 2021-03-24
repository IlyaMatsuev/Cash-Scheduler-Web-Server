using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CashSchedulerWebServer.Auth.Contracts;
using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Exceptions;
using CashSchedulerWebServer.Models;
using CashSchedulerWebServer.Services.Contracts;
using HotChocolate.Subscriptions;

namespace CashSchedulerWebServer.Services.Notifications
{
    public class UserNotificationService : IUserNotificationService
    {
        private IContextProvider ContextProvider { get; }
        private ITopicEventSender EventSender { get; }
        private int UserId { get; }

        public UserNotificationService(
            IContextProvider contextProvider,
            IUserContext userContext,
            ITopicEventSender eventSender)
        {
            ContextProvider = contextProvider;
            EventSender = eventSender;
            UserId = userContext.GetUserId();
        }


        public IEnumerable<UserNotification> GetAll()
        {
            return ContextProvider.GetRepository<IUserNotificationRepository>().GetAll();
        }

        public int GetUnreadCount()
        {
            return ContextProvider.GetRepository<IUserNotificationRepository>().GetUnread().Count();
        }

        public async Task<UserNotification> Create(UserNotification notification)
        {
            notification.User ??= ContextProvider.GetRepository<IUserRepository>().GetByKey(UserId);

            var createdNotification = await ContextProvider.GetRepository<IUserNotificationRepository>().Create(notification);

            await EventSender.SendAsync($"OnUserNotification_{notification.User.Id}", createdNotification);

            return createdNotification;
        }

        public Task<UserNotification> Update(UserNotification notification)
        {
            var notificationRepository = ContextProvider.GetRepository<IUserNotificationRepository>();

            var targetNotification = notificationRepository.GetByKey(notification.Id);
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

            return notificationRepository.Update(targetNotification);
        }

        public Task<UserNotification> ToggleRead(int id, bool read)
        {
            var notificationRepository = ContextProvider.GetRepository<IUserNotificationRepository>();

            var notification = notificationRepository.GetByKey(id);

            notification.IsRead = read;

            return notificationRepository.Update(notification);
        }

        public Task<UserNotification> Delete(int id)
        {
            var notificationRepository = ContextProvider.GetRepository<IUserNotificationRepository>();

            var targetNotification = notificationRepository.GetByKey(id);
            if (targetNotification == null)
            {
                throw new CashSchedulerException("There is no such notification");
            }

            return notificationRepository.Delete(id);
        }
    }
}
