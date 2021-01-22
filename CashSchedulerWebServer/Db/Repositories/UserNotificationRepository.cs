using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Models;
using CashSchedulerWebServer.Utils;
using GraphQL;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CashSchedulerWebServer.Db.Repositories
{
    public class UserNotificationRepository : IUserNotificationRepository
    {
        private ISubject<UserNotification> NotificationStream { get; set; }
        private CashSchedulerContext Context { get; set; }
        private IContextProvider ContextProvider { get; set; }
        private ClaimsPrincipal User { get; set; }
        private int? UserId => Convert.ToInt32(User?.Claims.FirstOrDefault(claim => claim.Type == "Id")?.Value ?? "-1");

        public UserNotificationRepository(CashSchedulerContext context, IHttpContextAccessor httpAccessor, IContextProvider contextProvider)
        {
            Context = context;
            ContextProvider = contextProvider;
            User = httpAccessor.HttpContext.User;
            NotificationStream = new ReplaySubject<UserNotification>(1);
        }


        public IObservable<UserNotification> GetLast()
        {
            return NotificationStream.AsObservable();
        }

        public IEnumerable<UserNotification> GetAll()
        {
            return Context.UserNotifications.Where(n => n.CreatedFor.Id == UserId)
                .Include(n => n.CreatedFor);
        }

        public IEnumerable<UserNotification> GetAllUnread()
        {
            return Context.UserNotifications.Where(n => n.CreatedFor.Id == UserId && !n.IsRead)
                .Include(n => n.CreatedFor);
        }

        public UserNotification GetById(int id)
        {
            return Context.UserNotifications.Where(n => n.Id == id && n.CreatedFor.Id == UserId)
                   .Include(n => n.CreatedFor)
                   .FirstOrDefault();
        }

        public async Task<UserNotification> Create(UserNotification notification)
        {
            ModelValidator.ValidateModelAttributes(notification);

            if (notification.CreatedFor == null)
            {
                notification.CreatedFor = ContextProvider.GetRepository<IUserRepository>().GetById((int)UserId);
            }

            Context.UserNotifications.Add(notification);
            await Context.SaveChangesAsync();
            NotificationStream.OnNext(notification);

            return GetById(notification.Id);
        }

        public async Task<UserNotification> Update(UserNotification notification)
        {
            var targetNotification = GetById(notification.Id);
            if (targetNotification == null)
            {
                throw new ExecutionError("There is no such notification");
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
                throw new ExecutionError("There is no such notification");
            }

            Context.UserNotifications.Remove(targetNotification);
            await Context.SaveChangesAsync();

            return targetNotification;
        }
    }
}
