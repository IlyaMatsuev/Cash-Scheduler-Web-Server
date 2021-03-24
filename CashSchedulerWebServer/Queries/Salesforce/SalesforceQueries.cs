using HotChocolate;
using HotChocolate.Types;
using Microsoft.Extensions.Configuration;

namespace CashSchedulerWebServer.Queries.Salesforce
{
    [ExtendObjectType(Name = "Query")]
    public class SalesforceQueries
    {
        [GraphQLNonNullType]
        public string SalesforceApiEndpoint([Service] IConfiguration configuration)
        {
            return configuration["WebServices:SalesforceApi:Endpoint"];
        }
    }
}
