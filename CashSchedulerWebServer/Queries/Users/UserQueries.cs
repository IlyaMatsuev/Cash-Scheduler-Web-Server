using System.Threading.Tasks;
using CashSchedulerWebServer.Auth;
using CashSchedulerWebServer.Auth.Contracts;
using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Models;
using CashSchedulerWebServer.Services.Contracts;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Types;

namespace CashSchedulerWebServer.Queries.Users
{
    [ExtendObjectType(Name = "Query")]
    public class UserQueries
    {
        [Authorize(Policy = AuthOptions.AUTH_POLICY)]
        public User User([Service] IContextProvider contextProvider)
        {
            return contextProvider.GetService<IUserService>().GetById();
        }

        [Authorize(Policy = AuthOptions.AUTH_POLICY, Roles = new []{AuthOptions.USER_ROLE})]
        public Task<string> AppToken([Service] IAuthService authService)
        {
            return authService.GenerateAppToken();
        }

        public Task<string> CheckEmail([Service] IAuthService authService, [GraphQLNonNullType] string email)
        {
            return authService.CheckEmail(email);
        }

        public Task<string> CheckCode(
            [Service] IAuthService authService,
            [GraphQLNonNullType] string email,
            [GraphQLNonNullType] string code)
        {
            return authService.CheckCode(email, code);
        }
    }
}
