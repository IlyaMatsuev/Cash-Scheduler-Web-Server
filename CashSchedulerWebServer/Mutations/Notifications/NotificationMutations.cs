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
        [Authorize(Policy = AuthOptions.AUTH_POLICY)]
        public Task<UserNotification> ToggleReadNotification(
            [Service] IContextProvider contextProvider,
            [GraphQLNonNullType] int id,
            [GraphQLNonNullType] bool read)
        {
            return contextProvider.GetService<IUserNotificationService>().ToggleRead(id, read);
        }
    }
}
