using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CashSchedulerWebServer.Auth.Contracts;
using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Models;
using CashSchedulerWebServer.Services.Contracts;
using CashSchedulerWebServer.Services.Transactions;
using CashSchedulerWebServer.Services.Users;
using Microsoft.Extensions.Configuration;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace CashSchedulerWebServer.Tests.Services
{
    public class TransactionServiceTest
    {
        /*private const int TESTING_USER_ID = 1;
        
        private ITransactionService TransactionService { get; }
        private IUserService UserService { get; }
        private Mock<IUserRepository> UserRepository { get; }
        private Mock<ITransactionRepository> TransactionRepository { get; }
        private Mock<ICategoryRepository> CategoryRepository { get; }
        private Mock<IContextProvider> ContextProvider { get; }
        private Mock<IUserContext> UserContext { get; }
        
        public TransactionServiceTest()
        {
            ContextProvider = new Mock<IContextProvider>();
            TransactionRepository = new Mock<ITransactionRepository>();
            CategoryRepository = new Mock<ICategoryRepository>();
            UserRepository = new Mock<IUserRepository>();
            UserContext = new Mock<IUserContext>();
            
            UserContext.Setup(c => c.GetUserId()).Returns(TESTING_USER_ID);

            ContextProvider
                .Setup(c => c.GetRepository<ITransactionRepository>())
                .Returns(TransactionRepository.Object);
            
            ContextProvider
                .Setup(c => c.GetRepository<ICategoryRepository>())
                .Returns(CategoryRepository.Object);
            
            ContextProvider
                .Setup(c => c.GetRepository<IUserRepository>())
                .Returns(UserRepository.Object);


            TransactionService = new TransactionService(ContextProvider.Object, UserContext.Object);
            
            UserService = new UserService(
                ContextProvider.Object,
                new ConfigurationBuilder().Build(),
                UserContext.Object
            );
        }


        [Fact]
        public async Task Create_ReturnsNewTransaction()
        {
            string usersJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"Users.json");
            var user = JsonConvert.DeserializeObject<List<User>>(usersJson).First(u => u.Id == TESTING_USER_ID);
            
            string categoriesJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"Categories.json");
            var category = JsonConvert.DeserializeObject<List<Category>>(categoriesJson)
                .First(c => !c.IsCustom || c.User.Id == TESTING_USER_ID);

            double initBalance = user.Balance;
            const string newTitle = "Test title";
            const double newAmount = 101;
            
            var newTransaction = new Transaction
            {
                Title = newTitle,
                Amount = newAmount,
                CategoryId = category.Id
            };
            
            UserRepository.Setup(u => u.GetByKey(TESTING_USER_ID)).Returns(user);

            UserRepository.Setup(u => u.Update(user)).ReturnsAsync(user);
            
            CategoryRepository.Setup(c => c.GetByKey(category.Id)).Returns(category);

            TransactionRepository.Setup(t => t.Create(newTransaction)).ReturnsAsync(newTransaction);

            ContextProvider
                .Setup(c => c.GetService<IUserService>())
                .Returns(UserService);
            

            var resultTransaction = await TransactionService.Create(newTransaction);
            
            
            Assert.NotNull(resultTransaction);
            Assert.NotNull(resultTransaction.User);
            Assert.NotNull(resultTransaction.Category);
            Assert.Equal(TESTING_USER_ID, resultTransaction.User.Id);
            Assert.Equal(category.Id, resultTransaction.Category.Id);
            Assert.Equal(newTitle, resultTransaction.Title);
            Assert.Equal(newAmount, resultTransaction.Amount);
            Assert.Equal(DateTime.Today, resultTransaction.Date);

            if (category.Type.Name == TransactionType.Options.Income.ToString())
            {
                Assert.Equal(initBalance + newAmount, resultTransaction.User.Balance);
            }
            else if (category.Type.Name == TransactionType.Options.Expense.ToString())
            {
                Assert.Equal(initBalance - newAmount, resultTransaction.User.Balance);
            }
        }
        
        [Fact]
        public async Task Update_ReturnsUpdatedTransaction()
        {
            string usersJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"Users.json");
            var user = JsonConvert.DeserializeObject<List<User>>(usersJson).First(u => u.Id == TESTING_USER_ID);
            
            string categoriesJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"Categories.json");
            var category = JsonConvert.DeserializeObject<List<Category>>(categoriesJson)
                .First(c => c.IsCustom && c.User.Id == TESTING_USER_ID);
            
            string transactionsJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"Transactions.json");
            var transaction = JsonConvert.DeserializeObject<List<Transaction>>(transactionsJson)
                .First(u => u.User.Id == TESTING_USER_ID && u.Category.Id == category.Id);

            double initBalance = user.Balance;
            double initTransactionAmount = transaction.Amount;
            const string newTitle = "Test title";
            const double newAmount = 101;
            // Taking future time
            DateTime newDate = DateTime.Today.AddDays(2);

            // Just because "user" and "category" variables contain more relationships
            transaction.User = user;
            transaction.Category = category;
            
            var newTransaction = new Transaction
            {
                Id = transaction.Id,
                Title = newTitle,
                Amount = newAmount,
                Date = newDate,
                User = transaction.User,
                Category = transaction.Category
            };

            UserRepository.Setup(u => u.Update(user)).ReturnsAsync(user);

            TransactionRepository.Setup(t => t.GetByKey(newTransaction.Id)).Returns(transaction);
            
            TransactionRepository.Setup(t => t.Update(transaction)).ReturnsAsync(transaction);

            ContextProvider
                .Setup(c => c.GetService<IUserService>())
                .Returns(UserService);
            

            var resultTransaction = await TransactionService.Update(newTransaction);
            
            
            Assert.NotNull(resultTransaction);
            Assert.NotNull(resultTransaction.User);
            Assert.NotNull(resultTransaction.Category);
            Assert.Equal(TESTING_USER_ID, resultTransaction.User.Id);
            Assert.Equal(category.Id, resultTransaction.Category.Id);
            Assert.Equal(newTitle, resultTransaction.Title);
            Assert.Equal(newAmount, resultTransaction.Amount);
            Assert.Equal(newDate, resultTransaction.Date);

            if (category.Type.Name == TransactionType.Options.Income.ToString())
            {
                Assert.Equal(initBalance - initTransactionAmount, resultTransaction.User.Balance);
            }
            else if (category.Type.Name == TransactionType.Options.Expense.ToString())
            {
                Assert.Equal(initBalance + initTransactionAmount, resultTransaction.User.Balance);
            }
        }
        
        [Fact]
        public async Task Delete_ReturnsDeletedTransaction()
        {
            string usersJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"Users.json");
            var user = JsonConvert.DeserializeObject<List<User>>(usersJson).First(u => u.Id == TESTING_USER_ID);
            
            string categoriesJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"Categories.json");
            var category = JsonConvert.DeserializeObject<List<Category>>(categoriesJson)
                .First(c => c.IsCustom && c.User.Id == TESTING_USER_ID);
            
            string transactionsJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"Transactions.json");
            var transaction = JsonConvert.DeserializeObject<List<Transaction>>(transactionsJson)
                .First(u => u.User.Id == TESTING_USER_ID && u.Category.Id == category.Id);

            double initBalance = user.Balance;

            // Just because "user" and "category" variables contain more relationships
            transaction.User = user;
            transaction.Category = category;

            UserRepository.Setup(u => u.Update(user)).ReturnsAsync(user);

            TransactionRepository.Setup(t => t.GetByKey(transaction.Id)).Returns(transaction);
            
            TransactionRepository.Setup(t => t.Delete(transaction.Id)).ReturnsAsync(transaction);

            ContextProvider
                .Setup(c => c.GetService<IUserService>())
                .Returns(UserService);
            

            var resultTransaction = await TransactionService.Delete(transaction.Id);
            
            
            Assert.NotNull(resultTransaction);
            Assert.NotNull(resultTransaction.User);
            Assert.NotNull(resultTransaction.Category);
            Assert.Equal(TESTING_USER_ID, resultTransaction.User.Id);
            Assert.Equal(category.Id, resultTransaction.Category.Id);
            Assert.Equal(transaction.Title, resultTransaction.Title);
            Assert.Equal(transaction.Amount, resultTransaction.Amount);
            Assert.Equal(transaction.Date, resultTransaction.Date);

            if (category.Type.Name == TransactionType.Options.Income.ToString())
            {
                Assert.Equal(initBalance - transaction.Amount, resultTransaction.User.Balance);
            }
            else if (category.Type.Name == TransactionType.Options.Expense.ToString())
            {
                Assert.Equal(initBalance + transaction.Amount, resultTransaction.User.Balance);
            }
        }*/
    }
}
