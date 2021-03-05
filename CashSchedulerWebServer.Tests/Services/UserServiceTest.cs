using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using CashSchedulerWebServer.Auth.Contracts;
using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Models;
using CashSchedulerWebServer.Services.Contracts;
using CashSchedulerWebServer.Services.Users;
using Microsoft.Extensions.Configuration;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace CashSchedulerWebServer.Tests.Services
{
    public class UserServiceTest
    {
        /*private const int TESTING_USER_ID = 1;
        private const string HASH_SALT = "12345";

        private IUserService UserService { get; }
        private Mock<IUserRepository> UserRepository { get; }
        private Mock<IContextProvider> ContextProvider { get; }
        private Mock<IUserContext> UserContext { get; }

        public UserServiceTest()
        {
            ContextProvider = new Mock<IContextProvider>();
            UserRepository = new Mock<IUserRepository>();
            UserContext = new Mock<IUserContext>();

            UserContext.Setup(u => u.GetUserId()).Returns(TESTING_USER_ID);

            ContextProvider.Setup(c => c.GetRepository<IUserRepository>()).Returns(UserRepository.Object);

            UserService = new UserService(
                ContextProvider.Object,
                new ConfigurationBuilder()
                    .AddInMemoryCollection(new Dictionary<string, string>
                    {
                        {"App:Auth:PasswordSalt", HASH_SALT}
                    }).Build(),
                UserContext.Object
            );
        }


        [Fact]
        public async Task Update_ReturnsUpdatedUser()
        {
            string usersJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"Users.json");
            var user = JsonConvert.DeserializeObject<List<User>>(usersJson).First(u => u.Id == TESTING_USER_ID);

            const string newFirstName = "Test First Name";
            const string newLastName = "Test Last Name";
            const double newBalance = 10002;

            var newUser = new User
            {
                Id = user.Id,
                FirstName = newFirstName,
                LastName = newLastName,
                Balance = newBalance,
                Email = user.Email,
                Password = user.Password
            };

            UserRepository.Setup(u => u.GetByKey(TESTING_USER_ID)).Returns(user);

            UserRepository.Setup(u => u.Update(user)).ReturnsAsync(user);


            var resultUser = await UserService.Update(newUser);


            Assert.NotNull(resultUser);
            Assert.Equal(newFirstName, resultUser.FirstName);
            Assert.Equal(newLastName, resultUser.LastName);
            Assert.Equal(newBalance, resultUser.Balance);
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
            Assert.Equal(user.Balance, resultUser.Balance);
            Assert.Equal(user.Email, resultUser.Email);
            Assert.Equal(newHashedPassword, resultUser.Password);
        }

        [Fact]
        public async Task UpdateBalance_ReturnsUpdatedUserForNewTransaction()
        {
            string usersJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"Users.json");
            var user = JsonConvert.DeserializeObject<List<User>>(usersJson).First(u => u.Id == TESTING_USER_ID);
            
            string categoriesJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"Categories.json");
            var category = JsonConvert.DeserializeObject<List<Category>>(categoriesJson)
                .First(c => c.IsCustom && c.User.Id == TESTING_USER_ID);

            double initBalance = user.Balance;
            const string newTitle = "Test title";
            const double newAmount = 1002;
            DateTime newDate = DateTime.Today.AddDays(-1);

            var newTransaction = new Transaction
            {
                User = user,
                Category = category,
                Amount = newAmount,
                Title = newTitle,
                Date = newDate
            };

            UserRepository.Setup(u => u.Update(user)).ReturnsAsync(user);


            var resultUser = await UserService.UpdateBalance(newTransaction, null, true);


            Assert.NotNull(resultUser);
            Assert.Equal(user.FirstName, resultUser.FirstName);
            Assert.Equal(user.LastName, resultUser.LastName);
            Assert.Equal(user.Balance, resultUser.Balance);
            Assert.Equal(user.Email, resultUser.Email);
            
            if (category.Type.Name == TransactionType.Options.Income.ToString())
            {
                Assert.Equal(initBalance + newTransaction.Amount, resultUser.Balance);
            }
            else if (category.Type.Name == TransactionType.Options.Expense.ToString())
            {
                Assert.Equal(initBalance - newTransaction.Amount, resultUser.Balance);
            }
        }
        
        [Fact]
        public async Task UpdateBalance_ReturnsUpdatedUserForExistingTransaction()
        {
            string usersJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"Users.json");
            var user = JsonConvert.DeserializeObject<List<User>>(usersJson).First(u => u.Id == TESTING_USER_ID);
            
            string categoriesJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"Categories.json");
            var category = JsonConvert.DeserializeObject<List<Category>>(categoriesJson)
                .First(c => c.IsCustom && c.User.Id == TESTING_USER_ID);
            
            string transactionsJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"Transactions.json");
            var transaction = JsonConvert.DeserializeObject<List<Transaction>>(transactionsJson)
                .First(t => t.User.Id == TESTING_USER_ID && t.Category.Id == category.Id);

            transaction.User = user;
            transaction.Category = category;

            double initBalance = user.Balance;
            const string newTitle = "Test title";
            const double newAmount = 1002;
            DateTime newDate = DateTime.Today.AddDays(-1);

            var newTransaction = new Transaction
            {
                Id = transaction.Id,
                User = user,
                Category = category,
                Amount = newAmount,
                Title = newTitle,
                Date = newDate
            };

            UserRepository.Setup(u => u.Update(user)).ReturnsAsync(user);


            var resultUser = await UserService.UpdateBalance(newTransaction, transaction, isUpdate: true);


            Assert.NotNull(resultUser);
            Assert.Equal(user.FirstName, resultUser.FirstName);
            Assert.Equal(user.LastName, resultUser.LastName);
            Assert.Equal(user.Balance, resultUser.Balance);
            Assert.Equal(user.Email, resultUser.Email);
            
            if (category.Type.Name == TransactionType.Options.Income.ToString())
            {
                Assert.Equal(initBalance + (newTransaction.Amount - transaction.Amount), resultUser.Balance);
            }
            else if (category.Type.Name == TransactionType.Options.Expense.ToString())
            {
                Assert.Equal(initBalance - (newTransaction.Amount - transaction.Amount), resultUser.Balance);
            }
        }
        
        [Fact]
        public async Task UpdateBalance_ReturnsUpdatedUserForDeletedTransaction()
        {
            string usersJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"Users.json");
            var user = JsonConvert.DeserializeObject<List<User>>(usersJson).First(u => u.Id == TESTING_USER_ID);
            
            string categoriesJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"Categories.json");
            var category = JsonConvert.DeserializeObject<List<Category>>(categoriesJson)
                .First(c => c.IsCustom && c.User.Id == TESTING_USER_ID);
            
            string transactionsJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"Transactions.json");
            var transaction = JsonConvert.DeserializeObject<List<Transaction>>(transactionsJson)
                .First(t => t.User.Id == TESTING_USER_ID && t.Category.Id == category.Id);

            transaction.User = user;
            transaction.Category = category;

            double initBalance = user.Balance;

            UserRepository.Setup(u => u.Update(user)).ReturnsAsync(user);


            var resultUser = await UserService.UpdateBalance(transaction, transaction, isDelete: true);


            Assert.NotNull(resultUser);
            Assert.Equal(user.FirstName, resultUser.FirstName);
            Assert.Equal(user.LastName, resultUser.LastName);
            Assert.Equal(user.Balance, resultUser.Balance);
            Assert.Equal(user.Email, resultUser.Email);
            
            if (category.Type.Name == TransactionType.Options.Income.ToString())
            {
                Assert.Equal(initBalance - transaction.Amount, resultUser.Balance);
            }
            else if (category.Type.Name == TransactionType.Options.Expense.ToString())
            {
                Assert.Equal(initBalance + transaction.Amount, resultUser.Balance);
            }
        }*/
    }
}