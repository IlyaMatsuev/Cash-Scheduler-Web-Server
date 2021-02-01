using System.Linq;
using System.Threading.Tasks;
using CashSchedulerWebServer.Exceptions;
using HotChocolate.Resolvers;
using Microsoft.AspNetCore.Authorization;

namespace CashSchedulerWebServer.Auth.AuthorizationHandlers
{
    public class CashSchedulerAuthorizationHandler : AuthorizationHandler<CashSchedulerUserRequirement, IResolverContext>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context, 
            CashSchedulerUserRequirement requirement, 
            IResolverContext resource)
        {
            string userId = context.User.Claims
                .FirstOrDefault(claim => claim.Type == UserContext.ID_CLAIM_TYPE)?.Value ?? string.Empty;

            if (!string.IsNullOrEmpty(userId))
            {
                context.Succeed(requirement);
            }
            else
            {
                throw new CashSchedulerException("Unauthorized", "401");
            }

            return Task.CompletedTask;
        }
    }
}
