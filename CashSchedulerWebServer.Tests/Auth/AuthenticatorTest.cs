using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using CashSchedulerWebServer.Auth;
using CashSchedulerWebServer.Auth.Contracts;
using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Events.Contracts;
using CashSchedulerWebServer.Exceptions;
using CashSchedulerWebServer.Models;
using CashSchedulerWebServer.Notifications.Contracts;
using CashSchedulerWebServer.Services.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Logging;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace CashSchedulerWebServer.Tests.Auth
{
    public class AuthenticatorTest
    {
        /*private const int TESTING_USER_ID = 1;
        
        private Dictionary<string, string> Configurations { get; } = new()
        {
            {"App:Auth:SkipAuth", "false"},
            {"App:Auth:PasswordSalt", "12345"},
            {"App:Auth:AccessTokenSecret", "12345123451234512345123451234512345123451234512345"},
            {"App:Auth:RefreshTokenSecret", "54321543215432154321543215432154321543215432154321"},
            {"App:Auth:AccessTokenLifetime", "60"},
            {"App:Auth:RefreshTokenLifetime", "10080"},
            {"App:Auth:EmailVerificationTokenLifetime", "3"}
        };
        
        private IAuthenticator Authenticator { get; }
        private Mock<IContextProvider> ContextProvider { get; }
        private Mock<INotificator> Notificator { get; }
        private Mock<IEventManager> EventManager { get; }
        private Mock<IUserRepository> UserRepository { get; }
        private Mock<IUserService> UserService { get; }
        private Mock<IUserRefreshTokenService> UserRefreshTokenService { get; }
        private Mock<IUserEmailVerificationCodeService> UserEmailVerificationCodeService { get; }
        
        public AuthenticatorTest()
        {
            ContextProvider = new Mock<IContextProvider>();
            Notificator = new Mock<INotificator>();
            EventManager = new Mock<IEventManager>();
            UserRepository = new Mock<IUserRepository>();
            UserService = new Mock<IUserService>();
            UserRefreshTokenService = new Mock<IUserRefreshTokenService>();
            UserEmailVerificationCodeService = new Mock<IUserEmailVerificationCodeService>();

            ContextProvider
                .Setup(u => u.GetRepository<IUserRepository>())
                .Returns(UserRepository.Object);
            
            ContextProvider
                .Setup(c => c.GetService<IUserService>())
                .Returns(UserService.Object);
            
            ContextProvider
                .Setup(c => c.GetService<IUserRefreshTokenService>())
                .Returns(UserRefreshTokenService.Object);
            
            ContextProvider
                .Setup(c => c.GetService<IUserEmailVerificationCodeService>())
                .Returns(UserEmailVerificationCodeService.Object);

            Authenticator = new Authenticator(
                ContextProvider.Object,
                Notificator.Object,
                EventManager.Object,
                new ConfigurationBuilder().AddInMemoryCollection(Configurations).Build()
            );

            IdentityModelEventSource.ShowPII = true;
        }


        [Fact]
        public async Task Login_ReturnsTokens()
        {
            string usersJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"Users.json");
            var user = JsonConvert.DeserializeObject<List<User>>(usersJson).First(u => u.Id == TESTING_USER_ID);

            const string truePassword = "P@$$w0rd";

            using (var sha = SHA256.Create())
            {
                user.Password = Encoding.ASCII.GetString(
                    sha.ComputeHash(Encoding.ASCII.GetBytes(truePassword + Configurations["App:Auth:PasswordSalt"]))
                );
            }
            
            UserRepository.Setup(u => u.GetByEmail(user.Email)).Returns(user);


            var tokens = await Authenticator.Login(user.Email, truePassword);
            
            
            Assert.NotNull(tokens);
            Assert.NotNull(tokens.AccessToken);
            Assert.NotNull(tokens.RefreshToken);
        }
        
        [Fact]
        public async Task Login_ThrowsExceptionAboutCredentials()
        {
            string usersJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"Users.json");
            var user = JsonConvert.DeserializeObject<List<User>>(usersJson).First(u => u.Id == TESTING_USER_ID);

            const string fakeEmail = "ivan.ivanov@hacker.com";
            const string fakePassword = "P@$$w0rd";


            await Assert.ThrowsAsync<CashSchedulerException>(async () =>
            {
                await Authenticator.Login(fakeEmail, fakePassword);
            });
            await Assert.ThrowsAsync<CashSchedulerException>(async () =>
            {
                await Authenticator.Login(user.Email, fakePassword);
            });
            await Assert.ThrowsAsync<CashSchedulerException>(async () =>
            {
                await Authenticator.Login(user.Email, user.Password);
            });
        }

        [Fact]
        public async Task Logout_ReturnsLoggedOutUser()
        {
            string usersJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"Users.json");
            var user = JsonConvert.DeserializeObject<List<User>>(usersJson).First(u => u.Id == TESTING_USER_ID);

            var refreshToken = new UserRefreshToken
            {
                Id = 101,
                ExpiredDate = DateTime.Now.AddMinutes(5),
                Token = "1234567890123456789012345678901234567890",
                User = user
            };

            UserService.Setup(u => u.GetById()).Returns(user);

            UserRefreshTokenService.Setup(u => u.GetByUserId(user.Id)).Returns(refreshToken);
            
            UserRefreshTokenService.Setup(u => u.Delete(refreshToken.Id)).ReturnsAsync(refreshToken);


            var resultUser = await Authenticator.Logout();
            
            Assert.NotNull(resultUser);
        }
        
        [Fact]
        public async Task Logout_ThrowsExceptionAboutAuthorization()
        {
            string usersJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"Users.json");
            var user = JsonConvert.DeserializeObject<List<User>>(usersJson).First(u => u.Id == TESTING_USER_ID);


            await Assert.ThrowsAsync<CashSchedulerException>(async () =>
            {
                await Authenticator.Logout();
            });

            await Assert.ThrowsAsync<CashSchedulerException>(async () =>
            {
                UserService.Setup(u => u.GetById()).Returns(user);
                await Authenticator.Logout();
            });
        }

        [Fact]
        public async Task Register_ReturnsNewUser()
        {
            const string newFirstName = "Bob";
            const string newLastName = "Foo";
            const string newEmail = "bob.foo@gmail.com";
            const double newBalance = 1001;
            const string newPassword = "P@$$w0rd";

            var newUser = new User
            {
                FirstName = newFirstName,
                LastName = newLastName,
                Email = newEmail,
                Balance = newBalance,
                Password = newPassword
            };

            UserService.Setup(u => u.HasWithEmail(newEmail)).Returns(false);

            UserService.Setup(u => u.Create(newUser)).ReturnsAsync(newUser);


            var resultUser = await Authenticator.Register(newUser);


            Assert.NotNull(resultUser);
            Assert.Equal(newFirstName, resultUser.FirstName);
            Assert.Equal(newLastName, resultUser.LastName);
            Assert.Equal(newEmail, resultUser.Email);
            Assert.Equal(newBalance, resultUser.Balance);
        }
        
        [Fact]
        public async Task Register_ThrowsExceptionAboutCredentials()
        {
            const string newFirstName = "Bob";
            const string newEmail = "bob.foo@gmail.com";
            const string newPassword = "Password";

            var newUser = new User
            {
                FirstName = newFirstName,
                Email = newEmail,
                Password = newPassword
            };

            
            await Assert.ThrowsAsync<CashSchedulerException>(async () =>
            {
                UserService.Setup(u => u.HasWithEmail(newEmail)).Returns(true);
                await Authenticator.Register(newUser);
            });
            
            await Assert.ThrowsAsync<CashSchedulerException>(async () =>
            {
                UserService.Setup(u => u.HasWithEmail(newEmail)).Returns(false);
                await Authenticator.Register(newUser);
            });
        }

        [Fact]
        public async Task Token_ReturnsNewTokens()
        {
            string usersJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"Users.json");
            var user = JsonConvert.DeserializeObject<List<User>>(usersJson).First(u => u.Id == TESTING_USER_ID);

            var refreshToken = new UserRefreshToken
            {
                Id = 101,
                ExpiredDate = DateTime.Now.AddMinutes(5),
                Token = "1234567890123456789012345678901234567890",
                User = user
            };
            
            UserRepository.Setup(u => u.GetByEmail(user.Email)).Returns(user);

            UserRefreshTokenService.Setup(u => u.GetByUserId(user.Id)).Returns(refreshToken);


            var tokens = await Authenticator.Token(user.Email, refreshToken.Token);
            
            
            Assert.NotNull(tokens);
            Assert.NotNull(tokens.AccessToken);
            Assert.NotNull(tokens.RefreshToken);
        }
        
        [Fact]
        public async Task Token_ThrowsExceptionAboutRefreshToken()
        {
            string usersJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"Users.json");
            var user = JsonConvert.DeserializeObject<List<User>>(usersJson).First(u => u.Id == TESTING_USER_ID);
            
            var refreshToken = new UserRefreshToken
            {
                Id = 101,
                ExpiredDate = DateTime.Now.AddMinutes(5),
                Token = "1234567890123456789012345678901234567890",
                User = user
            };
            
            await Assert.ThrowsAsync<CashSchedulerException>(async () =>
            {
                await Authenticator.Token(user.Email, refreshToken.Token);
            });
            
            UserRepository.Setup(u => u.GetByEmail(user.Email)).Returns(user);
            
            UserRefreshTokenService.Setup(u => u.GetByUserId(user.Id)).Returns(refreshToken);

            await Assert.ThrowsAsync<CashSchedulerException>(async () =>
            {
                await Authenticator.Token(user.Email, refreshToken.Token + "1");
            });
        }

        [Fact]
        public async Task CheckEmail_ThrowsExceptionAboutTiming()
        {
            string usersJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"Users.json");
            var user = JsonConvert.DeserializeObject<List<User>>(usersJson).First(u => u.Id == TESTING_USER_ID);

            var emailVerificationCode = new UserEmailVerificationCode
            {
                Id = 131,
                ExpiredDate = DateTime.UtcNow.AddMinutes(1),
                User = user,
                Code = "123467"
            };
            
            UserRepository.Setup(u => u.GetByEmail(user.Email)).Returns(user);

            UserEmailVerificationCodeService
                .Setup(u => u.GetByUserId(user.Id))
                .Returns(emailVerificationCode);


            await Assert.ThrowsAsync<CashSchedulerException>(async () =>
            {
                await Authenticator.CheckEmail(user.Email);
            });
        }

        [Fact]
        public async Task CheckCode_ThrowsExceptionAboutWrongCode()
        {
            string usersJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"Users.json");
            var user = JsonConvert.DeserializeObject<List<User>>(usersJson).First(u => u.Id == TESTING_USER_ID);

            var emailVerificationCode = new UserEmailVerificationCode
            {
                Id = 131,
                ExpiredDate = DateTime.UtcNow.AddMinutes(-2),
                User = user,
                Code = "123467"
            };
            
            UserRepository.Setup(u => u.GetByEmail(user.Email)).Returns(user);

            await Assert.ThrowsAsync<CashSchedulerException>(async () =>
            {
                await Authenticator.CheckCode(user.Email, emailVerificationCode.Code);
            });

            UserEmailVerificationCodeService
                .Setup(u => u.GetByUserId(user.Id))
                .Returns(emailVerificationCode);
            
            await Assert.ThrowsAsync<CashSchedulerException>(async () =>
            {
                await Authenticator.CheckCode(user.Email, emailVerificationCode.Code);
            });
            
            await Assert.ThrowsAsync<CashSchedulerException>(async () =>
            {
                emailVerificationCode.ExpiredDate = emailVerificationCode.ExpiredDate.AddMinutes(3);
                await Authenticator.CheckCode(user.Email, "7654321");
            });
        }

        [Fact]
        public async Task ResetPassword_ReturnsUpdatedUser()
        {
            string usersJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"Users.json");
            var user = JsonConvert.DeserializeObject<List<User>>(usersJson).First(u => u.Id == TESTING_USER_ID);

            const string newPassword = "P@$$w0rd";
            
            var emailVerificationCode = new UserEmailVerificationCode
            {
                Id = 131,
                ExpiredDate = DateTime.UtcNow.AddMinutes(1),
                User = user,
                Code = "123467"
            };
            
            UserRepository.Setup(u => u.GetByEmail(user.Email)).Returns(user);

            UserService.Setup(u => u.UpdatePassword(user.Email, newPassword)).ReturnsAsync(user);
            
            UserEmailVerificationCodeService
                .Setup(u => u.GetByUserId(user.Id))
                .Returns(emailVerificationCode);


            var resultUser = await Authenticator.ResetPassword(user.Email, emailVerificationCode.Code, newPassword);
            
            
            Assert.NotNull(resultUser);
            Assert.Equal(user.FirstName, resultUser.FirstName);
            Assert.Equal(user.LastName, resultUser.LastName);
            Assert.Equal(user.Email, resultUser.Email);
            Assert.Equal(user.Balance, resultUser.Balance);
        }
        
        [Fact]
        public async Task ResetPassword_ThrowsExceptionAboutCredentials()
        {
            string usersJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"Users.json");
            var user = JsonConvert.DeserializeObject<List<User>>(usersJson).First(u => u.Id == TESTING_USER_ID);

            const string weekPassword = "password";
            
            var emailVerificationCode = new UserEmailVerificationCode
            {
                Id = 131,
                ExpiredDate = DateTime.UtcNow.AddMinutes(1),
                User = user,
                Code = "123467"
            };
            
            UserRepository.Setup(u => u.GetByEmail(user.Email)).Returns(user);

            UserEmailVerificationCodeService
                .Setup(u => u.GetByUserId(user.Id))
                .Returns(emailVerificationCode);


            await Assert.ThrowsAsync<CashSchedulerException>(async () =>
            {
                await Authenticator.ResetPassword(user.Email, emailVerificationCode.Code, weekPassword);
            });
        }*/
    }
}
