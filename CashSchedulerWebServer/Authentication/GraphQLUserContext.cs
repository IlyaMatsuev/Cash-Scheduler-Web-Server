using GraphQL.Authorization;
using System.Security.Claims;

namespace CashSchedulerWebServer.Authentication
{
    public class GraphQLUserContext : IProvideClaimsPrincipal
    {
        public ClaimsPrincipal User { get; set; }

        public GraphQLUserContext(ClaimsPrincipal user)
        {
            User = user;
        }
    }
}
