using GraphQL.Authorization;
using System.Collections.Generic;
using System.Security.Claims;

namespace CashSchedulerWebServer.Authentication
{
    public class GraphQLUserContext : Dictionary<string, object>, IProvideClaimsPrincipal
    {
        public ClaimsPrincipal User { get; set; }

        public GraphQLUserContext(ClaimsPrincipal user)
        {
            User = user;
        }
    }
}
