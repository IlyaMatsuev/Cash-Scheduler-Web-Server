using CashSchedulerWebServer.Authentication.Contracts;
using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Exceptions;
using CashSchedulerWebServer.Models;
using CashSchedulerWebServer.Notifications;
using CashSchedulerWebServer.Notifications.Contracts;
using CashSchedulerWebServer.Types;
using CashSchedulerWebServer.Utils;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CashSchedulerWebServer.Authentication
{
    public class Authenticator : IAuthenticator
    {
        private IContextProvider ContextProvider { get; set; }
        private INotificator Notificator { get; set; }
        private IConfiguration Configuration { get; set; }

        public Authenticator(IContextProvider contextProvider, INotificator notificator, IConfiguration configuration)
        {
            ContextProvider = contextProvider;
            Notificator = notificator;
            Configuration = configuration;
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
            if (user == null || user.Password != password.Hash(Configuration))
            {
                throw new CashSchedulerException("Invalid email or password", new string[] { nameof(email), nameof(password) });
            }

            var accessToken = user.GenerateToken(AuthOptions.TokenType.ACCESS, Configuration);
            var refreshToken = user.GenerateToken(AuthOptions.TokenType.REFRESH, Configuration);

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
                var refreshTokenRepository = ContextProvider.GetRepository<IUserRefreshTokenRepository>();
                var refreshToken = refreshTokenRepository.GetByUserId(user.Id);
                if (refreshToken == null)
                {
                    throw new CashSchedulerException("You are already logged out");
                }
                else
                {
                    await refreshTokenRepository.Delete(refreshToken.Id);
                }
            }
            return user;
        }

        public async Task<User> Register(User newUser)
        {
            var userRepository = ContextProvider.GetRepository<IUserRepository>();

            if (userRepository.HasUserWithEmail(newUser.Email))
            {
                throw new CashSchedulerException("User with the same email has been already registered", new string[] { "email" });
            }

            if (string.IsNullOrEmpty(newUser.Password))
            {
                throw new CashSchedulerException("Password is a required field", new string[] { "password" });
            }

            if (!Regex.IsMatch(newUser.Password, AuthOptions.PASSWORD_REGEX))
            {
                throw new CashSchedulerException(
                    "Your password is too week. Consider to choose something with upper and lower case, digits and special characters with min and max length of 8 and 15",
                    new string[] { "password" }
                );
            }

            return await userRepository.Create(newUser);
        }

        public async Task<AuthTokensType> Token(string email, string refreshToken)
        {
            var user = ContextProvider.GetRepository<IUserRepository>().GetUserByEmail(email);
            if (user == null)
            {
                throw new CashSchedulerException("There is no such user", new string[] { nameof(email) });
            }

            var refreshTokenRepository = ContextProvider.GetRepository<IUserRefreshTokenRepository>();
            var userRefreshToken = refreshTokenRepository.GetByUserId(user.Id);
            if (userRefreshToken == null || userRefreshToken.Token != refreshToken)
            {
                throw new CashSchedulerException("Invalid refresh token", new string[] { nameof(refreshToken) });
            }

            var newAccessToken = user.GenerateToken(AuthOptions.TokenType.ACCESS, Configuration);
            var newRefreshToken = user.GenerateToken(AuthOptions.TokenType.REFRESH, Configuration);

            userRefreshToken.Token = newRefreshToken.token;

            await refreshTokenRepository.Update(userRefreshToken);

            return new AuthTokensType(newAccessToken.token, newRefreshToken.token);
        }

        public async Task<string> CheckEmail(string email)
        {
            var user = ContextProvider.GetRepository<IUserRepository>().GetUserByEmail(email);
            if (user == null)
            {
                throw new CashSchedulerException("There is no such user", new[] { nameof(email) });
            }

            int allowedVerificationInterval = Convert.ToInt32(Configuration["App:Auth:EmailVerificationTokenLifetime"]);
            var verificationCodeRepository = ContextProvider.GetRepository<IUserEmailVerificationCodeRepository>();

            var existingVerificationCode = verificationCodeRepository.GetByUserId(user.Id);

            if (existingVerificationCode != null)
            {
                TimeSpan difference = DateTime.UtcNow.Subtract(existingVerificationCode.ExpiredDate);

                if (difference.TotalSeconds < 0 && Math.Abs(difference.TotalSeconds) < allowedVerificationInterval * 60)
                {
                    throw new CashSchedulerException(
                        $"We already sent you a code. You can request it again in: {Math.Abs(difference.Minutes)}:{Math.Abs(difference.Seconds)}",
                        new[] { nameof(email) }
                    );
                }
            }

            var verificationCode = await verificationCodeRepository.Update(
                new UserEmailVerificationCode(email.Code(Configuration),
                DateTime.UtcNow.AddMinutes(allowedVerificationInterval), user)
            );

            var notificationDelegator = new NotificationDelegator();
            var template = notificationDelegator.GetTemplate(
                NotificationTemplateType.VerificationCode,
                new Dictionary<string, string> { { "code", verificationCode.Code } }
            );
            await Notificator.SendEmail(user.Email, template);
            await ContextProvider.GetRepository<IUserNotificationRepository>().Create(new UserNotification
            {
                Title = template.Subject,
                Content = template.Body,
                CreatedFor = user
            });

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

            if (verificationCode.ExpiredDate < DateTime.UtcNow)
            {
                throw new CashSchedulerException("This code has been expired", new[] { nameof(code) });
            }

            if (verificationCode.Code != code)
            {
                throw new CashSchedulerException("The code is not valid", new[] { nameof(code) });
            }

            return await Task.FromResult(email);
        }

        public async Task<User> ResetPassword(string email, string code, string password)
        {
            await CheckCode(email, code);

            if (string.IsNullOrEmpty(password))
            {
                throw new CashSchedulerException("Password is a required field", new string[] { nameof(password) });
            }

            if (!Regex.IsMatch(password, AuthOptions.PASSWORD_REGEX))
            {
                throw new CashSchedulerException(
                    "Your password is too week. Consider to choose something with upper and lower case, digits and special characters with min and max length of 8 and 15",
                    new string[] { nameof(password) }
                );
            }

            var user = await ContextProvider.GetRepository<IUserRepository>().UpdatePassword(email, password);

            var verificationCodeRepository = ContextProvider.GetRepository<IUserEmailVerificationCodeRepository>();
            var verificationCode = verificationCodeRepository.GetByUserId(user.Id);
            await verificationCodeRepository.Delete(verificationCode.Id);
            return user;
        }
    }
}
