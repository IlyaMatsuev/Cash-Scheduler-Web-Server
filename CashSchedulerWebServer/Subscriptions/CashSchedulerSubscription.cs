using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Models;
using CashSchedulerWebServer.Types;
using GraphQL.Resolvers;
using GraphQL.Subscription;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace CashSchedulerWebServer.Subscriptions
{
    public class CashSchedulerSubscription : ObjectGraphType
    {
        public CashSchedulerSubscription(IContextProvider contextProvider)
        {
            AddField(new EventStreamFieldType
            {
                Name = "notificationAdded",
                Type = typeof(UserNotificationType),
                Resolver = new FuncFieldResolver<UserNotification>(context => context.Source as UserNotification),
                Subscriber = new EventStreamResolver<UserNotification>(context => contextProvider.GetRepository<IUserNotificationRepository>().GetLast())
            });
        }
    }
}
