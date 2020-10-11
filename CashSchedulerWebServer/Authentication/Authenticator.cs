using CashSchedulerWebServer.Authentication.Contracts;
using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Models;
using CashSchedulerWebServer.Types;
using CashSchedulerWebServer.Utils;
using GraphQL;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CashSchedulerWebServer.Authentication
{
    public class Authenticator : IAuthenticator
    {
        private IContextProvider ContextProvider { get; set; }

        public Authenticator(IContextProvider contextProvider)
        {
            ContextProvider = contextProvider;
        }


        public async Task<AuthTokensType> Login(string email, string password)
        {
            var userRepo = ContextProvider.GetRepository<IUserRepository>();
            User user = userRepo.GetUserByEmail(email);
            if (user == null || user.Password != password.Hash())
            {
                throw new ExecutionError("Invalid email or password");
            }

            var accessToken = user.GenerateToken(AuthOptions.TokenType.ACCESS);
            var refreshToken = user.GenerateToken(AuthOptions.TokenType.REFRESH);

            await ContextProvider.GetRepository<IUserRefreshTokenRepository>().Update(new UserRefreshToken(refreshToken.token, refreshToken.expiresIn, user));

            return new AuthTokensType(accessToken.token, refreshToken.token);
        }

        public async Task<User> Logout()
        {
            var user = ContextProvider.GetRepository<IUserRepository>().GetById();
            if (user == null)
            {
                throw new ExecutionError("You are not authenticated yet")
                {
                    Code = "authorization"
                };
            }
            else
            {
                var refreshToken = ContextProvider.GetRepository<IUserRefreshTokenRepository>().GetByUserId(user.Id);
                if (refreshToken == null)
                {
                    throw new ExecutionError("You are already logged out");
                }
                else
                {
                    await ContextProvider.GetRepository<IUserRefreshTokenRepository>().Delete(refreshToken.Id);
                }
            }
            return user;
        }

        public async Task<User> Register(User newUser)
        {
            if (ContextProvider.GetRepository<IUserRepository>().HasUserWithEmail(newUser.Email))
            {
                throw new ExecutionError("User with the same email has been already registered");
            }

            return await ContextProvider.GetRepository<IUserRepository>().Create(newUser);
        }

        public async Task<AuthTokensType> Token(string email, string refreshToken)
        {
            var user = ContextProvider.GetRepository<IUserRepository>().GetUserByEmail(email);
            if (user == null)
            {
                throw new ExecutionError("There is no such user");
            }

            var userRefreshToken = ContextProvider.GetRepository<IUserRefreshTokenRepository>().GetByUserId(user.Id);
            if (userRefreshToken == null || userRefreshToken.Token != refreshToken)
            {
                throw new ExecutionError("Invalid refresh token");
            }

            var newAccessToken = user.GenerateToken(AuthOptions.TokenType.ACCESS);
            var newRefreshToken = user.GenerateToken(AuthOptions.TokenType.REFRESH);

            userRefreshToken.Token = newRefreshToken.token;

            await ContextProvider.GetRepository<IUserRefreshTokenRepository>().Update(userRefreshToken);

            return new AuthTokensType(newAccessToken.token, newRefreshToken.token);
        }

        public bool HasAccess(string accessToken)
        {
            var tokenClaims = accessToken.EvaluateToken();
            
            string expiresIn = tokenClaims.FirstOrDefault(claim => claim.Type == "ExpirationDateTime")?.Value ?? string.Empty;
            string userId = tokenClaims.FirstOrDefault(claim => claim.Type == "Id")?.Value ?? string.Empty;

            bool userValid = !string.IsNullOrEmpty(userId) && ContextProvider.GetRepository<IUserRepository>().GetById(Convert.ToInt32(userId)) != null;
            bool expired = string.IsNullOrEmpty(expiresIn) || DateTime.Parse(expiresIn) < DateTime.UtcNow;

            if (!userValid)
            {
                throw new ExecutionError("Access token is invalid")
                {
                    Code = "redirect_login"
                };
            }
            if (expired)
            {
                throw new ExecutionError("Access token is expired")
                {
                    Code = "authorization"
                };
            }

            return true;
        }
    }
}
