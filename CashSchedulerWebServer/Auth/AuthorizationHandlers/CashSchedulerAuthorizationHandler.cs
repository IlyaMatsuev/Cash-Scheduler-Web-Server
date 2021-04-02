using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CashSchedulerWebServer.Exceptions;
using HotChocolate.Resolvers;
using Microsoft.AspNetCore.Authorization;
using AuthorizeAttribute = HotChocolate.AspNetCore.Authorization.AuthorizeAttribute;

namespace CashSchedulerWebServer.Auth.AuthorizationHandlers
{
    public class CashSchedulerAuthorizationHandler : AuthorizationHandler<CashSchedulerUserRequirement, IResolverContext>
    {
        protected override Task HandleRequirementAsync(
            AuthorizationHandlerContext context, 
            CashSchedulerUserRequirement requirement, 
            IResolverContext resource)
        {
            if (HasUser(context) && RoleIsAllowed(context, resource))
            {
                context.Succeed(requirement);
            }
            else
            {
                throw new CashSchedulerException("Unauthorized", "401");
            }

            return Task.CompletedTask;
        }


        private bool HasUser(AuthorizationHandlerContext authorizationContext)
        {
            return !string.IsNullOrEmpty(authorizationContext.User.Claims.GetUserId());
        }

        private bool RoleIsAllowed(AuthorizationHandlerContext authorizationContext, IResolverContext resolverContext)
        {
            var allowedRoles = resolverContext.Field.Member?
                .GetCustomAttribute<AuthorizeAttribute>()?.Roles;

            return allowedRoles == null || allowedRoles.Any(r => authorizationContext.User.IsInRole(r));
        }
    }
}
