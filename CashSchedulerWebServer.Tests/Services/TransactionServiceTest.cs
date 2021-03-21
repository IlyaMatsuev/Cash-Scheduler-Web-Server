using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CashSchedulerWebServer.Auth.Contracts;
using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Events.Contracts;
using CashSchedulerWebServer.Exceptions;
using CashSchedulerWebServer.Models;
using CashSchedulerWebServer.Services.Contracts;
using CashSchedulerWebServer.Services.Transactions;
using CashSchedulerWebServer.Services.Wallets;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace CashSchedulerWebServer.Tests.Services
{
    public class TransactionServiceTest
    {
        private const int TESTING_USER_ID = 1;

        private ITransactionService TransactionService { get; }
        private IWalletService WalletService { get; }
        private Mock<IEventManager> EventManager { get; }
        private Mock<IUserRepository> UserRepository { get; }
        private Mock<IWalletRepository> WalletRepository { get; }
        private Mock<ITransactionRepository> TransactionRepository { get; }
        private Mock<ICategoryRepository> CategoryRepository { get; }
        private Mock<IContextProvider> ContextProvider { get; }
        private Mock<IUserContext> UserContext { get; }
        
        public TransactionServiceTest()
        {
            ContextProvider = new Mock<IContextProvider>();
            EventManager = new Mock<IEventManager>();
            TransactionRepository = new Mock<ITransactionRepository>();
            CategoryRepository = new Mock<ICategoryRepository>();
            UserRepository = new Mock<IUserRepository>();
            WalletRepository = new Mock<IWalletRepository>();
            UserContext = new Mock<IUserContext>();
            
            UserContext.Setup(c => c.GetUserId()).Returns(TESTING_USER_ID);

            ContextProvider
                .Setup(c => c.GetRepository<ITransactionRepository>())
                .Returns(TransactionRepository.Object);

            ContextProvider
                .Setup(c => c.GetRepository<IWalletRepository>())
                .Returns(WalletRepository.Object);

            ContextProvider
                .Setup(c => c.GetRepository<ICategoryRepository>())
                .Returns(CategoryRepository.Object);

            ContextProvider
                .Setup(c => c.GetRepository<IUserRepository>())
                .Returns(UserRepository.Object);


            TransactionService = new TransactionService(
                ContextProvider.Object,
                UserContext.Object,
                EventManager.Object
            );

            WalletService = new WalletService(
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

            double initBalance = wallet.Balance;
            const string newTitle = "Test title";
            const double newAmount = 101;

            var transaction = new Transaction
            {
                Title = newTitle,
                Amount = newAmount,
                CategoryId = category.Id,
                WalletId = wallet.Id
            };

            UserRepository.Setup(u => u.GetByKey(TESTING_USER_ID)).Returns(user);

            UserRepository.Setup(u => u.Update(user)).ReturnsAsync(user);

            WalletRepository.Setup(w => w.GetByKey(wallet.Id)).Returns(wallet);

            CategoryRepository.Setup(c => c.GetByKey(category.Id)).Returns(category);

            TransactionRepository.Setup(t => t.Create(transaction)).ReturnsAsync(transaction);

            ContextProvider
                .Setup(c => c.GetService<IWalletService>())
                .Returns(WalletService);


            var resultTransaction = await TransactionService.Create(transaction);


            Assert.NotNull(resultTransaction);
            Assert.NotNull(resultTransaction.User);
            Assert.NotNull(resultTransaction.Category);
            Assert.NotNull(resultTransaction.Wallet);
            Assert.Equal(TESTING_USER_ID, resultTransaction.User.Id);
            Assert.Equal(category.Id, resultTransaction.Category.Id);
            Assert.Equal(wallet.Id, resultTransaction.Wallet.Id);
            Assert.Equal(newTitle, resultTransaction.Title);
            Assert.Equal(newAmount, resultTransaction.Amount);
            Assert.Equal(DateTime.Today, resultTransaction.Date);

            if (category.Type.Name == TransactionType.Options.Income.ToString())
            {
                Assert.Equal(initBalance + newAmount, resultTransaction.Wallet.Balance);
            }
            else if (category.Type.Name == TransactionType.Options.Expense.ToString())
            {
                Assert.Equal(initBalance - newAmount, resultTransaction.Wallet.Balance);
            }
        }
        
        [Fact]
        public async Task Create_ThrowsExceptionAboutBalance()
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
            double amount = 101 + wallet.Balance;

            var transaction = new Transaction
            {
                Title = newTitle,
                Amount = amount,
                CategoryId = category.Id,
                WalletId = wallet.Id
            };

            UserRepository.Setup(u => u.GetByKey(TESTING_USER_ID)).Returns(user);

            UserRepository.Setup(u => u.Update(user)).ReturnsAsync(user);

            WalletRepository.Setup(w => w.GetByKey(wallet.Id)).Returns(wallet);

            CategoryRepository.Setup(c => c.GetByKey(category.Id)).Returns(category);


            await Assert.ThrowsAsync<CashSchedulerException>(async () =>
            {
                await TransactionService.Create(transaction);
            });
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

            string transactionsJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"Transactions.json");
            var transaction = JsonConvert.DeserializeObject<List<Transaction>>(transactionsJson)
                .First(u => u.User.Id == TESTING_USER_ID && u.Category.Id == category.Id);

            double initBalance = wallet.Balance;
            double initTransactionAmount = transaction.Amount;
            const string newTitle = "Test title";
            const double newAmount = 101;
            // Taking future time
            DateTime date = DateTime.Today.AddDays(2);

            // Just because "user", "wallet" and "category" variables contain more relationships
            transaction.User = user;
            transaction.Category = category;
            transaction.Wallet = wallet;

            var updateTransaction = new Transaction
            {
                Id = transaction.Id,
                Title = newTitle,
                Amount = newAmount,
                Date = date,
                User = transaction.User,
                Category = transaction.Category,
                Wallet = transaction.Wallet
            };

            WalletRepository.Setup(w => w.Update(wallet)).ReturnsAsync(wallet);

            TransactionRepository.Setup(t => t.GetByKey(updateTransaction.Id)).Returns(transaction);

            TransactionRepository.Setup(t => t.Update(transaction)).ReturnsAsync(transaction);

            ContextProvider
                .Setup(c => c.GetService<IWalletService>())
                .Returns(WalletService);


            var resultTransaction = await TransactionService.Update(updateTransaction);


            Assert.NotNull(resultTransaction);
            Assert.NotNull(resultTransaction.User);
            Assert.NotNull(resultTransaction.Category);
            Assert.NotNull(resultTransaction.Wallet);
            Assert.Equal(TESTING_USER_ID, resultTransaction.User.Id);
            Assert.Equal(category.Id, resultTransaction.Category.Id);
            Assert.Equal(wallet.Id, resultTransaction.Wallet.Id);
            Assert.Equal(newTitle, resultTransaction.Title);
            Assert.Equal(newAmount, resultTransaction.Amount);
            Assert.Equal(date, resultTransaction.Date);

            if (category.Type.Name == TransactionType.Options.Income.ToString())
            {
                Assert.Equal(initBalance - initTransactionAmount, resultTransaction.Wallet.Balance);
            }
            else if (category.Type.Name == TransactionType.Options.Expense.ToString())
            {
                Assert.Equal(initBalance + initTransactionAmount, resultTransaction.Wallet.Balance);
            }
        }
        
        [Fact]
        public async Task Update_ThrowsExceptionAboutBalance()
        {
            string usersJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"Users.json");
            var user = JsonConvert.DeserializeObject<List<User>>(usersJson).First(u => u.Id == TESTING_USER_ID);

            string categoriesJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"Categories.json");
            var category = JsonConvert.DeserializeObject<List<Category>>(categoriesJson)
                .First(c => c.IsCustom && c.User.Id == TESTING_USER_ID && c.Type.Name == TransactionType.Options.Expense.ToString());

            string walletsJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"Wallets.json");
            var wallet = JsonConvert.DeserializeObject<List<Wallet>>(walletsJson)
                .First(w => w.IsDefault);

            string transactionsJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"Transactions.json");
            var transaction = JsonConvert.DeserializeObject<List<Transaction>>(transactionsJson)
                .First(u => u.User.Id == TESTING_USER_ID && u.Category.Id == category.Id);

            const string newTitle = "Test title";
            double amount = 101 + transaction.Amount + wallet.Balance;
            // Taking future time
            DateTime date = DateTime.Today.AddDays(2);

            // Just because "user", "wallet" and "category" variables contain more relationships
            transaction.User = user;
            transaction.Category = category;
            transaction.Wallet = wallet;

            var updateTransaction = new Transaction
            {
                Id = transaction.Id,
                Title = newTitle,
                Amount = amount,
                Date = date,
                User = transaction.User,
                Category = transaction.Category,
                Wallet = transaction.Wallet
            };

            WalletRepository.Setup(w => w.Update(wallet)).ReturnsAsync(wallet);

            TransactionRepository.Setup(t => t.GetByKey(updateTransaction.Id)).Returns(transaction);


            await Assert.ThrowsAsync<CashSchedulerException>(async () =>
            {
                await TransactionService.Update(updateTransaction);
            });
        }
        
        [Fact]
        public async Task Delete_ReturnsDeletedTransaction()
        {
            string usersJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"Users.json");
            var user = JsonConvert.DeserializeObject<List<User>>(usersJson).First(u => u.Id == TESTING_USER_ID);
            
            string categoriesJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"Categories.json");
            var category = JsonConvert.DeserializeObject<List<Category>>(categoriesJson)
                .First(c => c.IsCustom && c.User.Id == TESTING_USER_ID);

            string walletsJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"Wallets.json");
            var wallet = JsonConvert.DeserializeObject<List<Wallet>>(walletsJson)
                .First(w => w.IsDefault);
            
            string transactionsJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"Transactions.json");
            var transaction = JsonConvert.DeserializeObject<List<Transaction>>(transactionsJson)
                .First(u => u.User.Id == TESTING_USER_ID && u.Category.Id == category.Id);

            double initBalance = wallet.Balance;

            transaction.User = user;
            transaction.Category = category;
            transaction.Wallet = wallet;

            WalletRepository.Setup(w => w.Update(wallet)).ReturnsAsync(wallet);

            TransactionRepository.Setup(t => t.GetByKey(transaction.Id)).Returns(transaction);

            TransactionRepository.Setup(t => t.Delete(transaction.Id)).ReturnsAsync(transaction);

            ContextProvider
                .Setup(c => c.GetService<IWalletService>())
                .Returns(WalletService);


            var resultTransaction = await TransactionService.Delete(transaction.Id);


            Assert.NotNull(resultTransaction);
            Assert.NotNull(resultTransaction.User);
            Assert.NotNull(resultTransaction.Category);
            Assert.NotNull(resultTransaction.Wallet);
            Assert.Equal(TESTING_USER_ID, resultTransaction.User.Id);
            Assert.Equal(category.Id, resultTransaction.Category.Id);
            Assert.Equal(wallet.Id, resultTransaction.Wallet.Id);
            Assert.Equal(transaction.Title, resultTransaction.Title);
            Assert.Equal(transaction.Amount, resultTransaction.Amount);
            Assert.Equal(transaction.Date, resultTransaction.Date);

            if (category.Type.Name == TransactionType.Options.Income.ToString())
            {
                Assert.Equal(initBalance - transaction.Amount, resultTransaction.Wallet.Balance);
            }
            else if (category.Type.Name == TransactionType.Options.Expense.ToString())
            {
                Assert.Equal(initBalance + transaction.Amount, resultTransaction.Wallet.Balance);
            }
        }
    }
}
