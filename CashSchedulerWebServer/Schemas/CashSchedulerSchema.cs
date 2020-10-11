using CashSchedulerWebServer.Mutations;
using CashSchedulerWebServer.Queries;
using GraphQL;

namespace CashSchedulerWebServer.Schemas
{
    public class CashSchedulerSchema : GraphQL.Types.Schema
    {
        public CashSchedulerSchema(IDependencyResolver resolver) : base(resolver)
        {
            Query = resolver.Resolve<CashSchedulerQuery>();
            Mutation = resolver.Resolve<CashSchedulerMutation>();
        }
    }
}
