using CashSchedulerWebServer.Mutations;
using CashSchedulerWebServer.Queries;
using GraphQL;
using GraphQL.Types;

namespace CashSchedulerWebServer.Schemas
{
    public class CashSchedulerSchema : Schema
    {
        public CashSchedulerSchema(IDependencyResolver resolver) : base(resolver)
        {
            Query = resolver.Resolve<CashSchedulerQuery>();
            Mutation = resolver.Resolve<CashSchedulerMutation>();
        }
    }
}
