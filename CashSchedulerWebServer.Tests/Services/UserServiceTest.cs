using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using CashSchedulerWebServer.Auth.Contracts;
using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Events.Contracts;
using CashSchedulerWebServer.Models;
using CashSchedulerWebServer.Services.Contracts;
using CashSchedulerWebServer.Services.Users;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace CashSchedulerWebServer.Tests.Services
{
    public class UserServiceTest
    {
        private const int TESTING_USER_ID = 1;
        private const string HASH_SALT = "12345";

        private IUserService UserService { get; }
        private Mock<IUserRepository> UserRepository { get; }
        private Mock<IContextProvider> ContextProvider { get; }
        private Mock<IEventManager> EventManager { get; }
        private Mock<IUserContext> UserContext { get; }
        private Mock<IServiceScopeFactory> ServiceScopeFactory { get; }

        public UserServiceTest()
        {
            ContextProvider = new Mock<IContextProvider>();
            EventManager = new Mock<IEventManager>();
            UserRepository = new Mock<IUserRepository>();
            UserContext = new Mock<IUserContext>();
            ServiceScopeFactory = new Mock<IServiceScopeFactory>();

            UserContext.Setup(u => u.GetUserId()).Returns(TESTING_USER_ID);

            ContextProvider.Setup(c => c.GetRepository<IUserRepository>()).Returns(UserRepository.Object);

            UserService = new UserService(
                ContextProvider.Object,
                new ConfigurationBuilder()
                    .AddInMemoryCollection(new Dictionary<string, string>
                    {
                        {"App:Auth:PasswordSalt", HASH_SALT}
                    }).Build(),
                UserContext.Object,
                EventManager.Object,
                ServiceScopeFactory.Object
            );
        }


        [Fact]
        public async Task Update_ReturnsUpdatedUser()
        {
            string usersJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"Users.json");
            var user = JsonConvert.DeserializeObject<List<User>>(usersJson).First(u => u.Id == TESTING_USER_ID);

            const string newFirstName = "Test First Name";
            const string newLastName = "Test Last Name";

            var newUser = new User
            {
                Id = user.Id,
                FirstName = newFirstName,
                LastName = newLastName,
                Email = user.Email,
                Password = user.Password
            };

            UserRepository.Setup(u => u.GetByKey(TESTING_USER_ID)).Returns(user);

            UserRepository.Setup(u => u.Update(user)).ReturnsAsync(user);


            var resultUser = await UserService.Update(newUser);


            Assert.NotNull(resultUser);
            Assert.Equal(newFirstName, resultUser.FirstName);
            Assert.Equal(newLastName, resultUser.LastName);
            Assert.Equal(user.Email, resultUser.Email);
            Assert.Equal(user.Password, resultUser.Password);
        }

        [Fact]
        public async Task UpdatePassword_ReturnsUpdatedUser()
        {
            string usersJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"Users.json");
            var user = JsonConvert.DeserializeObject<List<User>>(usersJson).First(u => u.Id == TESTING_USER_ID);

            const string newPassword = "Hello_world1@";
            string newHashedPassword;

            using (var sha = SHA256.Create())
            {
                newHashedPassword = Encoding.ASCII.GetString(
                    sha.ComputeHash(Encoding.ASCII.GetBytes(newPassword + HASH_SALT))
                );
            }

            UserRepository.Setup(u => u.GetByEmail(user.Email)).Returns(user);

            UserRepository.Setup(u => u.Update(user)).ReturnsAsync(user);


            var resultUser = await UserService.UpdatePassword(user.Email, newPassword);


            Assert.NotNull(resultUser);
            Assert.Equal(user.FirstName, resultUser.FirstName);
            Assert.Equal(user.LastName, resultUser.LastName);
            Assert.Equal(user.Email, resultUser.Email);
            Assert.Equal(newHashedPassword, resultUser.Password);
        }
    }
}