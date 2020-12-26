using CashSchedulerWebServer.Mutations;
using CashSchedulerWebServer.Queries;
using CashSchedulerWebServer.Subscriptions;
using GraphQL.Types;
using GraphQL.Utilities;
using System;

namespace CashSchedulerWebServer.Schemas
{
    public class CashSchedulerSchema : Schema
    {
        public CashSchedulerSchema(IServiceProvider provider) : base(provider)
        {
            Query = provider.GetRequiredService<CashSchedulerQuery>();
            Mutation = provider.GetRequiredService<CashSchedulerMutation>();
            Subscription = provider.GetRequiredService<CashSchedulerSubscription>();
        }
    }
}
