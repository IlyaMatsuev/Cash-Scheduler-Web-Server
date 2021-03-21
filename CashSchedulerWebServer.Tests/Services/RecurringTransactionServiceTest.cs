using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CashSchedulerWebServer.Auth.Contracts;
using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Events.Contracts;
using CashSchedulerWebServer.Models;
using CashSchedulerWebServer.Services.Contracts;
using CashSchedulerWebServer.Services.Transactions;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace CashSchedulerWebServer.Tests.Services
{
    public class RecurringTransactionServiceTest
    {
        private const int TESTING_USER_ID = 1;
        
        private IRecurringTransactionService RecurringTransactionService { get; }
        private Mock<IUserRepository> UserRepository { get; }
        private Mock<IEventManager> EventManager { get; }
        private Mock<IRegularTransactionRepository> RecurringTransactionRepository { get; }
        private Mock<ICategoryRepository> CategoryRepository { get; }
        private Mock<IWalletRepository> WalletRepository { get; }
        private Mock<IContextProvider> ContextProvider { get; }
        private Mock<IUserContext> UserContext { get; }

        
        public RecurringTransactionServiceTest()
        {
            ContextProvider = new Mock<IContextProvider>();
            EventManager = new Mock<IEventManager>();
            RecurringTransactionRepository = new Mock<IRegularTransactionRepository>();
            CategoryRepository = new Mock<ICategoryRepository>();
            WalletRepository = new Mock<IWalletRepository>();
            UserRepository = new Mock<IUserRepository>();
            UserContext = new Mock<IUserContext>();

            UserContext.Setup(c => c.GetUserId()).Returns(TESTING_USER_ID);

            ContextProvider
                .Setup(c => c.GetRepository<IRegularTransactionRepository>())
                .Returns(RecurringTransactionRepository.Object);

            ContextProvider
                .Setup(c => c.GetRepository<IWalletRepository>())
                .Returns(WalletRepository.Object);

            ContextProvider
                .Setup(c => c.GetRepository<ICategoryRepository>())
                .Returns(CategoryRepository.Object);

            ContextProvider
                .Setup(c => c.GetRepository<IUserRepository>())
                .Returns(UserRepository.Object);


            RecurringTransactionService = new RecurringTransactionService(
                ContextProvider.Object,
                UserContext.Object,
                EventManager.Object
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
            
            string walletsJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"Wallets.json");
            var wallet = JsonConvert.DeserializeObject<List<Wallet>>(walletsJson)
                .First(w => !w.IsDefault && w.User.Id == TESTING_USER_ID);

            const string newTitle = "Test title";
            const double newAmount = 101;
            string newInterval = RegularTransaction.IntervalOptions.Month.ToString();
            DateTime newNextDate = DateTime.Today.AddDays(7);

            var newTransaction = new RegularTransaction
            {
                Title = newTitle,
                Amount = newAmount,
                CategoryId = category.Id,
                NextTransactionDate = newNextDate,
                Interval = newInterval,
                WalletId = wallet.Id
            };

            UserRepository.Setup(u => u.GetByKey(TESTING_USER_ID)).Returns(user);

            CategoryRepository.Setup(c => c.GetByKey(category.Id)).Returns(category);

            RecurringTransactionRepository
                .Setup(t => t.Create(newTransaction))
                .ReturnsAsync(newTransaction);

            WalletRepository.Setup(w => w.GetByKey(wallet.Id)).Returns(wallet);


            var resultTransaction = await RecurringTransactionService.Create(newTransaction);


            Assert.NotNull(resultTransaction);
            Assert.NotNull(resultTransaction.User);
            Assert.NotNull(resultTransaction.Category);
            Assert.NotNull(resultTransaction.Wallet);
            Assert.Equal(TESTING_USER_ID, resultTransaction.User.Id);
            Assert.Equal(category.Id, resultTransaction.Category.Id);
            Assert.Equal(wallet.Id, resultTransaction.Wallet.Id);
            Assert.Equal(newTitle, resultTransaction.Title);
            Assert.Equal(newAmount, resultTransaction.Amount);
            Assert.Equal(newInterval, resultTransaction.Interval);
            Assert.Equal(DateTime.Today, resultTransaction.Date);
            Assert.Equal(newNextDate, resultTransaction.NextTransactionDate);
        }

        [Fact]
        public async Task Update_ReturnsUpdatedTransaction()
        {
            string usersJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"Users.json");
            var user = JsonConvert.DeserializeObject<List<User>>(usersJson).First(u => u.Id == TESTING_USER_ID);

            string categoriesJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"Categories.json");
            var category = JsonConvert.DeserializeObject<List<Category>>(categoriesJson)
                .First(c => c.IsCustom && c.User.Id == TESTING_USER_ID);

            string walletsJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"Wallets.json");
            var wallet = JsonConvert.DeserializeObject<List<Wallet>>(walletsJson)
                .First(w => w.IsDefault);

            string transactionsJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"RegularTransactions.json");
            var transaction = JsonConvert.DeserializeObject<List<RegularTransaction>>(transactionsJson)
                .First(u => u.User.Id == TESTING_USER_ID && u.Category.Id == category.Id && u.Wallet.Id == wallet.Id);

            const string newTitle = "Test title";
            const double newAmount = 101;

            // Just because "user" and "category" variables contain more relationships
            transaction.User = user;
            transaction.Category = category;
            transaction.Wallet = wallet;

            var newTransaction = new RegularTransaction
            {
                Id = transaction.Id,
                Title = newTitle,
                Amount = newAmount,
                Category = transaction.Category,
                Wallet = transaction.Wallet,
                User = transaction.User,
                NextTransactionDate = transaction.NextTransactionDate,
                Interval = transaction.Interval
            };

            CategoryRepository.Setup(c => c.GetByKey(category.Id)).Returns(category);

            RecurringTransactionRepository.Setup(t => t.GetByKey(newTransaction.Id)).Returns(transaction);

            RecurringTransactionRepository
                .Setup(t => t.Update(transaction))
                .ReturnsAsync(transaction);


            var resultTransaction = await RecurringTransactionService.Update(newTransaction);


            Assert.NotNull(resultTransaction);
            Assert.NotNull(resultTransaction.User);
            Assert.NotNull(resultTransaction.Category);
            Assert.NotNull(resultTransaction.Wallet);
            Assert.Equal(TESTING_USER_ID, resultTransaction.User.Id);
            Assert.Equal(category.Id, resultTransaction.Category.Id);
            Assert.Equal(wallet.Id, resultTransaction.Wallet.Id);
            Assert.Equal(newTitle, resultTransaction.Title);
            Assert.Equal(newAmount, resultTransaction.Amount);
            Assert.Equal(transaction.Interval, resultTransaction.Interval);
            Assert.Equal(transaction.Date, resultTransaction.Date);
            Assert.Equal(transaction.NextTransactionDate, resultTransaction.NextTransactionDate);
        }
        
        [Fact]
        public async Task Delete_ReturnsDeletedTransaction()
        {
            string transactionsJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"RegularTransactions.json");
            var transaction = JsonConvert.DeserializeObject<List<RegularTransaction>>(transactionsJson)
                .First(u => u.User.Id == TESTING_USER_ID);


            RecurringTransactionRepository.Setup(t => t.GetByKey(transaction.Id)).Returns(transaction);

            RecurringTransactionRepository
                .Setup(t => t.Delete(transaction.Id))
                .ReturnsAsync(transaction);


            var resultTransaction = await RecurringTransactionService.Delete(transaction.Id);


            Assert.NotNull(resultTransaction);
            Assert.Equal(transaction.Title, resultTransaction.Title);
            Assert.Equal(transaction.Amount, resultTransaction.Amount);
            Assert.Equal(transaction.Interval, resultTransaction.Interval);
            Assert.Equal(transaction.Date, resultTransaction.Date);
            Assert.Equal(transaction.NextTransactionDate, resultTransaction.NextTransactionDate);
        }
    }
}
