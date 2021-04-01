using System.Collections.Generic;
using CashSchedulerWebServer.Auth;
using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Models;
using CashSchedulerWebServer.Services.Contracts;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Types;

#nullable enable

namespace CashSchedulerWebServer.Queries.UserNotifications
{
    [ExtendObjectType(Name = "Query")]
    public class UserNotificationQueries
    {
        [Authorize(Policy = AuthOptions.AUTH_POLICY, Roles = new[] {AuthOptions.USER_ROLE})]
        public IEnumerable<UserNotification>? Notifications([Service] IContextProvider contextProvider)
        {
            return contextProvider.GetService<IUserNotificationService>().GetAll();
        }

        [GraphQLNonNullType]
        [Authorize(Policy = AuthOptions.AUTH_POLICY, Roles = new[] {AuthOptions.USER_ROLE})]
        public int UnreadNotificationsCount([Service] IContextProvider contextProvider)
        {
            return contextProvider.GetService<IUserNotificationService>().GetUnreadCount();
        }
    }
}
