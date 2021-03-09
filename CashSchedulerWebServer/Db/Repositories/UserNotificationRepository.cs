﻿using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Models;
using CashSchedulerWebServer.Utils;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CashSchedulerWebServer.Auth.Contracts;

namespace CashSchedulerWebServer.Db.Repositories
{
    public class UserNotificationRepository : IUserNotificationRepository
    {
        private CashSchedulerContext Context { get; }
        private int UserId { get; }

        public UserNotificationRepository(CashSchedulerContext context, IUserContext userContext)
        {
            Context = context;
            UserId = userContext.GetUserId();
        }


        public UserNotification GetByKey(int id)
        {
            return Context.UserNotifications
                .Where(n => n.Id == id && n.User.Id == UserId)
                .Include(n => n.User)
                .FirstOrDefault();
        }

        public IEnumerable<UserNotification> GetAll()
        {
            return Context.UserNotifications.Where(n => n.User.Id == UserId)
                .OrderByDescending(n => n.Id)
                .Include(n => n.User);
        }

        public IEnumerable<UserNotification> GetUnread()
        {
            return Context.UserNotifications.Where(n => n.User.Id == UserId && !n.IsRead)
                .OrderByDescending(n => n.Id)
                .Include(n => n.User);
        }

        public async Task<UserNotification> Create(UserNotification notification)
        {
            ModelValidator.ValidateModelAttributes(notification);

            await Context.UserNotifications.AddAsync(notification);
            await Context.SaveChangesAsync();

            return GetByKey(notification.Id);
        }

        public async Task<UserNotification> Update(UserNotification notification)
        {
            ModelValidator.ValidateModelAttributes(notification);

            Context.UserNotifications.Update(notification);
            await Context.SaveChangesAsync();

            return GetByKey(notification.Id);
        }

        public async Task<UserNotification> Delete(int id)
        {
            var notification = GetByKey(id);

            Context.UserNotifications.Remove(notification);
            await Context.SaveChangesAsync();

            return notification;
        }
    }
}
