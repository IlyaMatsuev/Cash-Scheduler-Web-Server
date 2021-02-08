using System.Collections.Generic;
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
            IUserContext userContext,
            IContextProvider contextProvider,
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

        public async Task<UserNotification> Create(UserNotification notification)
        {
            notification.User ??= ContextProvider.GetRepository<IUserRepository>().GetById(UserId);

            await EventSender.SendAsync($"OnNotificationForUser_{UserId}", notification);
            
            return await ContextProvider.GetRepository<IUserNotificationRepository>().Create(notification);
        }

        public Task<UserNotification> Update(UserNotification notification)
        {
            var notificationRepository = ContextProvider.GetRepository<IUserNotificationRepository>();
            
            var targetNotification = notificationRepository.GetById(notification.Id);
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

            return notificationRepository.Update(notification);
        }

        public Task<UserNotification> ToggleRead(int id, bool read)
        {
            return ContextProvider.GetRepository<IUserNotificationRepository>().Update(new UserNotification
            {
                Id = id,
                IsRead = read
            });
        }

        public Task<UserNotification> Delete(int id)
        {
            var notificationRepository = ContextProvider.GetRepository<IUserNotificationRepository>();
            
            var targetNotification = notificationRepository.GetById(id);
            if (targetNotification == null)
            {
                throw new CashSchedulerException("There is no such notification");
            }

            return notificationRepository.Delete(id);
        }
    }
}
