using System.Threading;
using System.Threading.Tasks;
using CashSchedulerWebServer.Auth.Contracts;
using CashSchedulerWebServer.Exceptions;
using CashSchedulerWebServer.Models;
using HotChocolate;
using HotChocolate.Execution;
using HotChocolate.Subscriptions;
using HotChocolate.Types;

namespace CashSchedulerWebServer.Subscriptions.Notifications
{
    [ExtendObjectType(Name = "Subscription")]
    public class UserNotificationSubscriptions
    {
        [SubscribeAndResolve]
        public async ValueTask<ISourceStream<UserNotification>> OnNotificationCreated(
            [Service] ITopicEventReceiver eventReceiver,
            [Service] IUserContext userContext,
            CancellationToken cancellationToken)
        {
            int userId = userContext.GetUserId();
            if (userId == -1)
            {
                throw new CashSchedulerException("Unauthorized", "401");
            }
            return await eventReceiver.SubscribeAsync<string, UserNotification>($"OnNotificationForUser_{userId}", cancellationToken);
        }
    }
}
