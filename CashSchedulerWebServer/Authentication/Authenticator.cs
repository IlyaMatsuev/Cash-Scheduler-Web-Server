using CashSchedulerWebServer.Authentication.Contracts;
using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Exceptions;
using CashSchedulerWebServer.Models;
using CashSchedulerWebServer.Types;
using CashSchedulerWebServer.Utils;
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
            if (string.IsNullOrEmpty(email))
            {
                throw new CashSchedulerException("Email is a required field for sign in", new string[] { nameof(email) });
            }
            if (string.IsNullOrEmpty(password))
            {
                throw new CashSchedulerException("Password is a required field for sign in", new string[] { nameof(password) });
            }

            User user = ContextProvider.GetRepository<IUserRepository>().GetUserByEmail(email);
            if (user == null || user.Password != password.Hash())
            //if (user == null || user.Password != password)
            {
                throw new CashSchedulerException("Invalid email or password", new string[] { nameof(email), nameof(password) });
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
                throw new CashSchedulerException("You are not authenticated yet", "authorization");
            }
            else
            {
                var refreshToken = ContextProvider.GetRepository<IUserRefreshTokenRepository>().GetByUserId(user.Id);
                if (refreshToken == null)
                {
                    throw new CashSchedulerException("You are already logged out");
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
                throw new CashSchedulerException("User with the same email has been already registered", new string[] { "email" });
            }

            return await ContextProvider.GetRepository<IUserRepository>().Create(newUser);
        }

        public async Task<AuthTokensType> Token(string email, string refreshToken)
        {
            var user = ContextProvider.GetRepository<IUserRepository>().GetUserByEmail(email);
            if (user == null)
            {
                throw new CashSchedulerException("There is no such user", new string[] { nameof(email) });
            }

            var userRefreshToken = ContextProvider.GetRepository<IUserRefreshTokenRepository>().GetByUserId(user.Id);
            if (userRefreshToken == null || userRefreshToken.Token != refreshToken)
            {
                throw new CashSchedulerException("Invalid refresh token", new string[] { nameof(refreshToken) });
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
                throw new CashSchedulerException("Access token is invalid");
            }
            if (expired)
            {
                throw new CashSchedulerException("Access token is expired", "authorization");
            }

            return true;
        }

        public async Task<string> CheckEmail(string email)
        {
            var user = ContextProvider.GetRepository<IUserRepository>().GetUserByEmail(email);
            if (user == null)
            {
                throw new CashSchedulerException("There is no such user", new[] { nameof(email) });
            }

            string code = email.Code();
            var verificationCode = await ContextProvider.GetRepository<IUserEmailVerificationCodeRepository>().Update(
                new UserEmailVerificationCode(code, DateTime.Now.AddMinutes(AuthOptions.EMAIL_VERIFICATION_CODE_LIFETIME), user)
            );
            Console.WriteLine("Code Id: " + verificationCode.Id);
            Console.WriteLine("Code: " + verificationCode.Code);

            // TODO: send email

            return email;
        }

        public async Task<string> CheckCode(string email, string code)
        {
            var user = ContextProvider.GetRepository<IUserRepository>().GetUserByEmail(email);
            if (user == null)
            {
                throw new CashSchedulerException("There is no such user", new[] { nameof(email) });
            }

            var verificationCode = ContextProvider.GetRepository<IUserEmailVerificationCodeRepository>().GetByUserId(user.Id);
            if (verificationCode == null)
            {
                throw new CashSchedulerException("We haven't sent you a code yet", new[] { nameof(email) });
            }

            if (verificationCode.ExpiredDate < DateTime.Now)
            {
                throw new CashSchedulerException("This code has been expired", new[] { nameof(code) });
            }

            if (verificationCode.Code != code)
            {
                throw new CashSchedulerException("The code is not valid", new[] { nameof(code) });
            }

            return email;
        }

        public async Task<User> ResetPassword(string email, string code, string password)
        {
            await CheckCode(email, code);
            var user = await ContextProvider.GetRepository<IUserRepository>().UpdatePassword(email, password);
            var verificationCode = ContextProvider.GetRepository<IUserEmailVerificationCodeRepository>().GetByUserId(user.Id);
            await ContextProvider.GetRepository<IUserEmailVerificationCodeRepository>().Delete(verificationCode.Id);
            return user;
        }
    }
}
