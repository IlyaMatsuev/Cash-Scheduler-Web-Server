using System.Threading.Tasks;
using CashSchedulerWebServer.Auth;
using CashSchedulerWebServer.Auth.Contracts;
using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Models;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Types;

namespace CashSchedulerWebServer.Mutations.Users
{
    [ExtendObjectType(Name = "Mutation")]
    public class UserMutations
    {
        [GraphQLNonNullType]
        public Task<User> Register([Service] IAuthenticator authenticator, [GraphQLNonNullType] NewUserInput user)
        {
            return authenticator.Register(new User
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Balance = user.Balance ?? default,
                Email = user.Email,
                Password = user.Password
            });
        }

        [GraphQLNonNullType]
        public Task<AuthTokens> Login([Service] IAuthenticator authenticator, [GraphQLNonNullType] string email, [GraphQLNonNullType] string password)
        {
            return authenticator.Login(email, password);
        }
        
        [GraphQLNonNullType]
        [Authorize(Policy = AuthOptions.AUTH_POLICY)]
        public Task<User> Logout([Service] IAuthenticator authenticator)
        {
            return authenticator.Logout();
        }
        
        [GraphQLNonNullType]
        public Task<AuthTokens> Token([Service] IAuthenticator authenticator, [GraphQLNonNullType] string email, [GraphQLNonNullType] string refreshToken)
        {
            return authenticator.Token(email, refreshToken);
        }
        
        [GraphQLNonNullType]
        public Task<User> ResetPassword(
            [Service] IAuthenticator authenticator,
            [GraphQLNonNullType] string email,
            [GraphQLNonNullType] string code,
            [GraphQLNonNullType] string password)
        {
            return authenticator.ResetPassword(email, code, password);
        }

        [GraphQLNonNullType]
        [Authorize(Policy = AuthOptions.AUTH_POLICY)]
        public Task<User> UpdateUser([Service] IContextProvider contextProvider, [GraphQLNonNullType] UpdateUserInput user)
        {
            return contextProvider.GetRepository<IUserRepository>().Update(new User
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Balance = user.Balance ?? default
            });
        }
    }
}
