using System.Threading.Tasks;
using CashSchedulerWebServer.Auth;
using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Models;
using CashSchedulerWebServer.Services.Contracts;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Types;

namespace CashSchedulerWebServer.Mutations.Notifications
{
    [ExtendObjectType(Name = "Mutation")]
    public class NotificationMutations
    {
        [GraphQLNonNullType]
        [Authorize(Policy = AuthOptions.AUTH_POLICY, Roles = new[] {AuthOptions.USER_ROLE})]
        public Task<UserNotification> ToggleReadNotification(
            [Service] IContextProvider contextProvider,
            [GraphQLNonNullType] int id,
            [GraphQLNonNullType] bool read)
        {
            return contextProvider.GetService<IUserNotificationService>().ToggleRead(id, read);
        }

        [GraphQLNonNullType]
        [Authorize(Roles = new[] {AuthOptions.SALESFORCE_ROLE})]
        public Task<UserNotification> CreateNotification(
            [Service] IContextProvider contextProvider,
            [GraphQLNonNullType] NewNotificationInput notification)
        {
            return contextProvider.GetService<IUserNotificationService>().Create(new UserNotification
            {
                Title = notification.Title,
                Content = notification.Content,
                IsRead = false,
                User = contextProvider.GetRepository<IUserRepository>().GetByKey(notification.UserId)
            });
        }
    }
}
