using System.Threading;
using System.Threading.Tasks;
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
            int userId,
            CancellationToken cancellationToken)
        {
            // The only way I found to make this a bit more secured is:
            // 1. Generate the token on the client side with the user id encrypted
            // 2. Send this token as a variable in a subscription
            // 3. Parse the token on the server side and subscribe to user notifications by its id
            return await eventReceiver.SubscribeAsync<string, UserNotification>($"OnUserNotification_{userId}", cancellationToken);
        }
    }
}
