using System.Threading.Tasks;
using CashSchedulerWebServer.Auth;
using CashSchedulerWebServer.Auth.Contracts;
using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Models;
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
            return contextProvider.GetRepository<IUserRepository>().GetById();
        }
        
        public Task<string> CheckEmail([Service] IAuthenticator authenticator, [GraphQLNonNullType] string email)
        {
            return authenticator.CheckEmail(email);
        }
        
        public Task<string> CheckCode([Service] IAuthenticator authenticator, [GraphQLNonNullType] string email, [GraphQLNonNullType] string code)
        {
            return authenticator.CheckCode(email, code);
        }
    }
}
