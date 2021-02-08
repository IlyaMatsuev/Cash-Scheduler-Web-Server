using System.Collections.Generic;
using CashSchedulerWebServer.Auth;
using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Models;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Types;

#nullable enable

namespace CashSchedulerWebServer.Queries.UserNotifications
{
    [ExtendObjectType(Name = "Query")]
    public class UserNotificationQueries
    {
        [Authorize(Policy = AuthOptions.AUTH_POLICY)]
        public IEnumerable<UserNotification>? AllNotifications([Service] IContextProvider contextProvider)
        {
            return contextProvider.GetRepository<IUserNotificationRepository>().GetAll();
        }
    }
}
