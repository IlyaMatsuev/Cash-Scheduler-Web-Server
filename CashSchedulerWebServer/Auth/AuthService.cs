using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CashSchedulerWebServer.Auth.Contracts;
using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Events;
using CashSchedulerWebServer.Events.Contracts;
using CashSchedulerWebServer.Exceptions;
using CashSchedulerWebServer.Models;
using CashSchedulerWebServer.Mutations.Users;
using CashSchedulerWebServer.Notifications;
using CashSchedulerWebServer.Notifications.Contracts;
using CashSchedulerWebServer.Services.Contracts;
using CashSchedulerWebServer.Utils;
using Microsoft.Extensions.Configuration;

namespace CashSchedulerWebServer.Auth
{
    public class AuthService : IAuthService
    {
        private IContextProvider ContextProvider { get; }
        private INotificator Notificator { get; }
        private IConfiguration Configuration { get; }
        private IEventManager EventManager { get; }

        public AuthService(
            IContextProvider contextProvider,
            INotificator notificator,
            IEventManager eventManager,
            IConfiguration configuration)
        {
            ContextProvider = contextProvider;
            Notificator = notificator;
            EventManager = eventManager;
            Configuration = configuration;
        }


        public async Task<AuthTokens> Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email))
            {
                throw new CashSchedulerException("Email is a required field for sign in", new[] {nameof(email)});
            }

            if (string.IsNullOrEmpty(password))
            {
                throw new CashSchedulerException("Password is a required field for sign in", new[] {nameof(password)});
            }

            var user = ContextProvider.GetRepository<IUserRepository>().GetByEmail(email);
            if (user == null || user.Password != password.Hash(Configuration))
            {
                throw new CashSchedulerException("Invalid email or password", new[] {nameof(email), nameof(password)});
            }

            var accessToken = user.GenerateToken(AuthOptions.TokenType.Access, Configuration);
            var refreshToken = user.GenerateToken(AuthOptions.TokenType.Refresh, Configuration);

            await ContextProvider.GetService<IUserRefreshTokenService>().Create(new UserRefreshToken
            {
                User = user,
                Token = refreshToken.token.Hash(Configuration),
                ExpiredDate = refreshToken.expiresIn,
                Type = (int) AuthOptions.TokenType.Refresh
            });

            await EventManager.FireEvent(EventAction.UserLogin, user);

            return new AuthTokens
            {
                AccessToken = accessToken.token,
                RefreshToken = refreshToken.token
            };
        }

        public async Task<AuthTokens> AppLogin(string appToken)
        {
            if (string.IsNullOrEmpty(appToken))
            {
                throw new CashSchedulerException("App token is required for connecting an app", new[] {nameof(appToken)});
            }

            var tokenClaims = appToken.EvaluateToken();
            if (!tokenClaims.IsTokenValid())
            {
                throw new CashSchedulerException("Access token is invalid", new[] {nameof(appToken)});
            }

            string role = tokenClaims.GetRole();
            if (role != AuthOptions.APP_ROLE)
            {
                throw new CashSchedulerException("Method allowed only for connected apps", new[] {nameof(appToken)});
            }

            var user = ContextProvider.GetRepository<IUserRepository>().GetByKey(Convert.ToInt32(tokenClaims.GetUserId()));
            if (user == null)
            {
                throw new CashSchedulerException("Invalid user id provided", new[] {nameof(appToken)});
            }

            var accessToken = user.GenerateToken(AuthOptions.TokenType.AppAccess, Configuration);
            var refreshToken = user.GenerateToken(AuthOptions.TokenType.AppRefresh, Configuration);

            await ContextProvider.GetService<IUserRefreshTokenService>().Create(new UserRefreshToken
            {
                User = user,
                Token = refreshToken.token.Hash(Configuration),
                ExpiredDate = refreshToken.expiresIn,
                Type = (int) AuthOptions.TokenType.AppRefresh
            });

            await EventManager.FireEvent(EventAction.UserLogin, user);

            return new AuthTokens
            {
                AccessToken = accessToken.token,
                RefreshToken = refreshToken.token
            };
        }

        public async Task<User> Logout()
        {
            var user = ContextProvider.GetService<IUserService>().GetById();
            if (user == null)
            {
                throw new CashSchedulerException("Unauthorized", "401");
            }

            await ContextProvider.GetService<IUserRefreshTokenService>().DeleteAllUserTokens(user.Id);

            return user;
        }

        public async Task<User> LogoutConnectedApps()
        {
            var user = ContextProvider.GetService<IUserService>().GetById();
            if (user == null)
            {
                throw new CashSchedulerException("Unauthorized", "401");
            }

            await ContextProvider.GetService<IUserRefreshTokenService>().DeleteAllAppTokens(user.Id);

            return user;
        }

        public async Task<User> Register(User user)
        {
            var userService = ContextProvider.GetService<IUserService>();

            if (userService.HasWithEmail(user.Email))
            {
                throw new CashSchedulerException(
                    "User with the same email has been already registered",
                    new[] {"email"}
                );
            }

            if (string.IsNullOrEmpty(user.Password))
            {
                throw new CashSchedulerException("Password is a required field", new[] {"password"});
            }

            if (!Regex.IsMatch(user.Password, AuthOptions.PASSWORD_REGEX))
            {
                throw new CashSchedulerException(
                    "Your password is too week. Consider to choose something with upper and lower case, " +
                    "digits and special characters with min and max length of 8 and 15",
                    new[] {"password"}
                );
            }

            var registeredUser = await userService.Create(user);

            await EventManager.FireEvent(EventAction.UserRegistered, registeredUser);

            return registeredUser;
        }

        public async Task<AuthTokens> Token(string email, string refreshToken)
        {
            var user = ContextProvider.GetRepository<IUserRepository>().GetByEmail(email);
            if (user == null)
            {
                throw new CashSchedulerException("There is no such user", new[] {nameof(email)});
            }

            var refreshTokenRepository = ContextProvider.GetRepository<IUserRefreshTokenRepository>();

            var userRefreshToken = refreshTokenRepository.GetByUserAndToken(user.Id, refreshToken.Hash(Configuration));
            if (userRefreshToken == null)
            {
                throw new CashSchedulerException("Invalid refresh token", new[] {nameof(refreshToken)});
            }

            var accessTokenType = AuthOptions.TokenType.Access;
            var refreshTokenType = AuthOptions.TokenType.Refresh;

            if (userRefreshToken.Type == (int) AuthOptions.TokenType.AppRefresh)
            {
                accessTokenType = AuthOptions.TokenType.AppAccess;
                refreshTokenType = AuthOptions.TokenType.AppRefresh;
            }

            var newAccessToken = user.GenerateToken(accessTokenType, Configuration);
            var newRefreshToken = user.GenerateToken(refreshTokenType, Configuration);

            userRefreshToken.Token = newRefreshToken.token.Hash(Configuration);
            userRefreshToken.ExpiredDate = newRefreshToken.expiresIn;

            await ContextProvider.GetService<IUserRefreshTokenService>().Update(userRefreshToken);

            await EventManager.FireEvent(EventAction.UserLogin, user);

            return new AuthTokens
            {
                AccessToken = newAccessToken.token,
                RefreshToken = newRefreshToken.token
            };
        }

        public Task<string> GenerateAppToken()
        {
            var user = ContextProvider.GetService<IUserService>().GetById();
            if (user == null)
            {
                throw new CashSchedulerException("Unauthorized", "401");
            }

            return Task.FromResult(
                user.GenerateToken(AuthOptions.TokenType.AppAccess, Configuration).token
            );
        }

        public async Task<string> CheckEmail(string email)
        {
            var user = ContextProvider.GetRepository<IUserRepository>().GetByEmail(email);
            if (user == null)
            {
                throw new CashSchedulerException("There is no such user", new[] {nameof(email)});
            }

            int allowedVerificationInterval = Convert.ToInt32(Configuration["App:Auth:EmailVerificationTokenLifetime"]);
            var verificationCodeService = ContextProvider.GetService<IUserEmailVerificationCodeService>();

            var existingVerificationCode = verificationCodeService.GetByUserId(user.Id);

            if (existingVerificationCode != null)
            {
                var difference = DateTime.UtcNow.Subtract(existingVerificationCode.ExpiredDate);

                if (difference.TotalSeconds < 0
                    && Math.Abs(difference.TotalSeconds) < allowedVerificationInterval * 60
                    && !bool.Parse(Configuration["App:Auth:SkipAuth"]))
                {
                    throw new CashSchedulerException(
                        "We already sent you a code. " +
                        $"You can request it again in: {Math.Abs(difference.Minutes)}:{Math.Abs(difference.Seconds)}",
                        new[] {nameof(email)}
                    );
                }
            }

            var verificationCode = await verificationCodeService.Update(new UserEmailVerificationCode(
                email.Code(Configuration),
                DateTime.UtcNow.AddMinutes(allowedVerificationInterval),
                user
            ));

            var notificationDelegator = new NotificationDelegator();
            var template = notificationDelegator.GetTemplate(
                NotificationTemplateType.VerificationCode,
                new Dictionary<string, string> {{"code", verificationCode.Code}}
            );
            await Notificator.SendEmail(user.Email, template);
            await ContextProvider.GetService<IUserNotificationService>().Create(new UserNotification
            {
                Title = template.Subject,
                Content = template.Body,
                User = user
            });

            return email;
        }

        public async Task<string> CheckCode(string email, string code)
        {
            var user = ContextProvider.GetRepository<IUserRepository>().GetByEmail(email);
            if (user == null)
            {
                throw new CashSchedulerException("There is no such user", new[] {nameof(email)});
            }

            var verificationCode = ContextProvider
                .GetService<IUserEmailVerificationCodeService>().GetByUserId(user.Id);

            if (verificationCode == null)
            {
                throw new CashSchedulerException("We haven't sent you a code yet", new[] {nameof(email)});
            }

            if (verificationCode.ExpiredDate < DateTime.UtcNow)
            {
                throw new CashSchedulerException("This code has been expired", new[] {nameof(code)});
            }

            if (verificationCode.Code != code)
            {
                throw new CashSchedulerException("The code is not valid", new[] {nameof(code)});
            }

            return await Task.FromResult(email);
        }

        public async Task<User> ResetPassword(string email, string code, string password)
        {
            await CheckCode(email, code);

            if (string.IsNullOrEmpty(password))
            {
                throw new CashSchedulerException("Password is a required field", new[] {nameof(password)});
            }

            if (!Regex.IsMatch(password, AuthOptions.PASSWORD_REGEX))
            {
                throw new CashSchedulerException(
                    "Your password is too week. Consider to choose something with upper and lower case, " +
                    "digits and special characters with min and max length of 8 and 15",
                    new[] {nameof(password)}
                );
            }

            var user = await ContextProvider.GetService<IUserService>().UpdatePassword(email, password);

            var verificationCodeService = ContextProvider.GetService<IUserEmailVerificationCodeService>();
            var verificationCode = verificationCodeService.GetByUserId(user.Id);

            await verificationCodeService.Delete(verificationCode.Id);

            return user;
        }
    }
}