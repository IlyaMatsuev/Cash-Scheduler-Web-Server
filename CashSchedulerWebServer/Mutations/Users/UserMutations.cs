using System.Threading.Tasks;
using CashSchedulerWebServer.Auth;
using CashSchedulerWebServer.Auth.Contracts;
using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Models;
using CashSchedulerWebServer.Services.Contracts;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Types;

namespace CashSchedulerWebServer.Mutations.Users
{
    [ExtendObjectType(Name = "Mutation")]
    public class UserMutations
    {
        [GraphQLNonNullType]
        public Task<User> Register([Service] IAuthService authService, [GraphQLNonNullType] NewUserInput user)
        {
            return authService.Register(new User
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Balance = user.Balance ?? default,
                Email = user.Email,
                Password = user.Password
            });
        }

        [GraphQLNonNullType]
        public Task<AuthTokens> Login(
            [Service] IAuthService authService,
            [GraphQLNonNullType] string email,
            [GraphQLNonNullType] string password)
        {
            return authService.Login(email, password);
        }

        [GraphQLNonNullType]
        public Task<AuthTokens> AppLogin([Service] IAuthService authService, [GraphQLNonNullType] string appToken)
        {
            return authService.AppLogin(appToken);
        }

        [GraphQLNonNullType]
        [Authorize(Policy = AuthOptions.AUTH_POLICY, Roles = new[] {AuthOptions.USER_ROLE})]
        public Task<User> Logout([Service] IAuthService authService)
        {
            return authService.Logout();
        }

        [GraphQLNonNullType]
        [Authorize(Policy = AuthOptions.AUTH_POLICY, Roles = new[] {AuthOptions.USER_ROLE})]
        public Task<User> LogoutConnectedApps([Service] IAuthService authService)
        {
            return authService.LogoutConnectedApps();
        }

        [GraphQLNonNullType]
        public Task<AuthTokens> Token(
            [Service] IAuthService authService,
            [GraphQLNonNullType] string email,
            [GraphQLNonNullType] string refreshToken)
        {
            return authService.Token(email, refreshToken);
        }

        [GraphQLNonNullType]
        public Task<User> ResetPassword(
            [Service] IAuthService authService,
            [GraphQLNonNullType] string email,
            [GraphQLNonNullType] string code,
            [GraphQLNonNullType] string password)
        {
            return authService.ResetPassword(email, code, password);
        }

        [GraphQLNonNullType]
        [Authorize(Policy = AuthOptions.AUTH_POLICY, Roles = new[] {AuthOptions.USER_ROLE})]
        public Task<User> UpdateUser(
            [Service] IContextProvider contextProvider,
            [GraphQLNonNullType] UpdateUserInput user)
        {
            return contextProvider.GetService<IUserService>().Update(new User
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Balance = user.Balance ?? default
            });
        }

        [GraphQLNonNullType]
        [Authorize(Policy = AuthOptions.AUTH_POLICY, Roles = new[] {AuthOptions.USER_ROLE})]
        public Task<User> DeleteUser(
            [Service] IContextProvider contextProvider,
            [GraphQLNonNullType] string password)
        {
            return contextProvider.GetService<IUserService>().Delete(password);
        }
    }
}
