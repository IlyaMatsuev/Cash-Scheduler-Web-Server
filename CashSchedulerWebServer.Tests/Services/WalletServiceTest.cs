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
using CashSchedulerWebServer.Services.Wallets;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace CashSchedulerWebServer.Tests.Services
{
    public class WalletServiceTest
    {
        private const int TESTING_USER_ID = 1;
        
        private IWalletService WalletService { get; }
        private Mock<IContextProvider> ContextProvider { get; }
        private Mock<IUserContext> UserContext { get; }
        private Mock<IEventManager> EventManager { get; }
        private Mock<IWalletRepository> WalletRepository { get; }
        private Mock<IUserRepository> UserRepository { get; }
        private Mock<ICurrencyRepository> CurrencyRepository { get; }

        public WalletServiceTest()
        {
            ContextProvider = new Mock<IContextProvider>();
            UserContext = new Mock<IUserContext>();
            EventManager = new Mock<IEventManager>();
            WalletRepository = new Mock<IWalletRepository>();
            UserRepository = new Mock<IUserRepository>();
            CurrencyRepository = new Mock<ICurrencyRepository>();

            ContextProvider.Setup(c => c.GetRepository<IWalletRepository>()).Returns(WalletRepository.Object);

            ContextProvider.Setup(c => c.GetRepository<IUserRepository>()).Returns(UserRepository.Object);

            ContextProvider.Setup(c => c.GetRepository<ICurrencyRepository>()).Returns(CurrencyRepository.Object);

            UserContext.Setup(u => u.GetUserId()).Returns(TESTING_USER_ID);

            WalletService = new WalletService(
                ContextProvider.Object, 
                UserContext.Object,
                EventManager.Object
            );
        }


        [Fact]
        public async Task Create_ReturnsNewWallet()
        {
            string usersJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"Users.json");
            var user = JsonConvert.DeserializeObject<List<User>>(usersJson).First(u => u.Id == TESTING_USER_ID);

            string currenciesJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"Currencies.json");
            var currencies = JsonConvert.DeserializeObject<List<Currency>>(currenciesJson);

            const string newName = "Testing Wallet";
            const double newBalance = 1233.11;
            const string newCurrencyAbbreviation = "USD";

            var newWallet = new Wallet
            {
                Name = newName,
                Balance = newBalance,
                CurrencyAbbreviation = newCurrencyAbbreviation,
                IsDefault = false
            };

            UserRepository.Setup(u => u.GetByKey(TESTING_USER_ID)).Returns(user);

            CurrencyRepository
                .Setup(c => c.GetByKey(newCurrencyAbbreviation))
                .Returns(currencies.First(cc => cc.Abbreviation == newCurrencyAbbreviation));

            WalletRepository.Setup(w => w.Create(newWallet)).ReturnsAsync(newWallet);


            var resultWallet = await WalletService.Create(newWallet);


            Assert.NotNull(resultWallet);
            Assert.NotNull(resultWallet.User);
            Assert.NotNull(resultWallet.Currency);
            Assert.Equal(TESTING_USER_ID, resultWallet.User.Id);
            Assert.Equal(newCurrencyAbbreviation, resultWallet.Currency.Abbreviation);
            Assert.Equal(newName, resultWallet.Name);
            Assert.Equal(newBalance, resultWallet.Balance);
            Assert.False(resultWallet.IsDefault);
        }

        [Fact]
        public async Task Update_ReturnsUpdatedWallet()
        {
            string currenciesJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"Currencies.json");
            var currencies = JsonConvert.DeserializeObject<List<Currency>>(currenciesJson);
            
            string walletsJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"Wallets.json");
            var wallet = JsonConvert.DeserializeObject<List<Wallet>>(walletsJson).First(w => w.User.Id == TESTING_USER_ID);

            const string newName = "Testing Wallet";
            const string sourceCurrencyAbbreviation = "USD";
            const string newCurrencyAbbreviation = "BYN";
            const float usdToBynRate = 2.61f;
            double initBalance = wallet.Balance;
            double resultBalance = initBalance * usdToBynRate;

            wallet.Currency = currencies.First(c => c.Abbreviation == sourceCurrencyAbbreviation);

            var newWallet = new Wallet
            {
                Id = wallet.Id,
                Name = newName,
                CurrencyAbbreviation = newCurrencyAbbreviation
            };

            CurrencyRepository
                .Setup(c => c.GetByKey(newCurrencyAbbreviation))
                .Returns(currencies.First(cc => cc.Abbreviation == newCurrencyAbbreviation));

            WalletRepository.Setup(w => w.GetByKey(newWallet.Id)).Returns(wallet);

            WalletRepository.Setup(w => w.Update(wallet)).ReturnsAsync(wallet);


            var resultWallet = await WalletService.Update(newWallet, true, usdToBynRate);


            Assert.NotNull(resultWallet);
            Assert.NotNull(resultWallet.Currency);
            Assert.Equal(newCurrencyAbbreviation, resultWallet.Currency.Abbreviation);
            Assert.Equal(newName, resultWallet.Name);
            Assert.Equal(resultBalance, resultWallet.Balance);
            Assert.Equal(wallet.IsDefault, resultWallet.IsDefault);
        }

        [Fact]
        public async Task UpdateBalance_ReturnsUpdatedWalletForNewTransaction()
        {
            string usersJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"Users.json");
            var user = JsonConvert.DeserializeObject<List<User>>(usersJson).First(u => u.Id == TESTING_USER_ID);

            string walletsJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"Wallets.json");
            var wallet = JsonConvert.DeserializeObject<List<Wallet>>(walletsJson).First(w => w.User.Id == TESTING_USER_ID);

            string categoriesJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"Categories.json");
            var category = JsonConvert.DeserializeObject<List<Category>>(categoriesJson)
                .First(c => c.IsCustom && c.User.Id == TESTING_USER_ID);

            double initBalance = wallet.Balance;
            const string newTitle = "Test title";
            const double newAmount = 1002;
            DateTime newDate = DateTime.Today.AddDays(-1);

            var newTransaction = new Transaction
            {
                User = user,
                Category = category,
                Wallet = wallet,
                Amount = newAmount,
                Title = newTitle,
                Date = newDate
            };

            WalletRepository.Setup(w => w.Update(wallet)).ReturnsAsync(wallet);


            var resultWallet = await WalletService.UpdateBalance(newTransaction, null, true);


            Assert.NotNull(resultWallet);
            Assert.NotNull(resultWallet.Currency);
            Assert.Equal(wallet.Currency.Abbreviation, resultWallet.Currency.Abbreviation);
            Assert.Equal(wallet.Name, resultWallet.Name);
            Assert.Equal(wallet.IsDefault, resultWallet.IsDefault);
            
            if (category.Type.Name == TransactionType.Options.Income.ToString())
            {
                Assert.Equal(initBalance + newTransaction.Amount, resultWallet.Balance);
            }
            else if (category.Type.Name == TransactionType.Options.Expense.ToString())
            {
                Assert.Equal(initBalance - newTransaction.Amount, resultWallet.Balance);
            }
        }

        [Fact]
        public async Task UpdateBalance_ReturnsUpdatedWalletForExistingTransaction()
        {
            string usersJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"Users.json");
            var user = JsonConvert.DeserializeObject<List<User>>(usersJson).First(u => u.Id == TESTING_USER_ID);

            string walletsJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"Wallets.json");
            var wallet = JsonConvert.DeserializeObject<List<Wallet>>(walletsJson).First(w => w.User.Id == TESTING_USER_ID);

            string categoriesJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"Categories.json");
            var category = JsonConvert.DeserializeObject<List<Category>>(categoriesJson)
                .First(c => c.IsCustom && c.User.Id == TESTING_USER_ID);

            string transactionsJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"Transactions.json");
            var transaction = JsonConvert.DeserializeObject<List<Transaction>>(transactionsJson)
                .First(t => t.User.Id == TESTING_USER_ID && t.Category.Id == category.Id);

            transaction.User = user;
            transaction.Category = category;
            transaction.Wallet = wallet;

            double initBalance = wallet.Balance;
            const string newTitle = "Test title";
            const double newAmount = 100;
            DateTime newDate = DateTime.Today.AddDays(-1);

            var newTransaction = new Transaction
            {
                Id = transaction.Id,
                User = user,
                Category = category,
                Wallet = wallet,
                Amount = newAmount,
                Title = newTitle,
                Date = newDate
            };

            WalletRepository.Setup(w => w.Update(wallet)).ReturnsAsync(wallet);


            var resultWallet = await WalletService.UpdateBalance(newTransaction, transaction, isUpdate: true);


            Assert.NotNull(resultWallet);
            Assert.NotNull(resultWallet.Currency);
            Assert.Equal(wallet.Currency.Abbreviation, resultWallet.Currency.Abbreviation);
            Assert.Equal(wallet.Name, resultWallet.Name);
            Assert.Equal(wallet.IsDefault, resultWallet.IsDefault);

            if (category.Type.Name == TransactionType.Options.Income.ToString())
            {
                Assert.Equal(initBalance - transaction.Amount - newAmount, resultWallet.Balance);
            }
            else if (category.Type.Name == TransactionType.Options.Expense.ToString())
            {
                Assert.Equal(initBalance + transaction.Amount - newAmount, resultWallet.Balance);
            }
        }

        [Fact]
        public async Task UpdateBalance_ReturnsUpdatedWalletForDeletedTransaction()
        {
            string usersJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"Users.json");
            var user = JsonConvert.DeserializeObject<List<User>>(usersJson).First(u => u.Id == TESTING_USER_ID);

            string walletsJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"Wallets.json");
            var wallet = JsonConvert.DeserializeObject<List<Wallet>>(walletsJson).First(w => w.User.Id == TESTING_USER_ID);

            string categoriesJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"Categories.json");
            var category = JsonConvert.DeserializeObject<List<Category>>(categoriesJson)
                .First(c => c.IsCustom && c.User.Id == TESTING_USER_ID);

            string transactionsJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"Transactions.json");
            var transaction = JsonConvert.DeserializeObject<List<Transaction>>(transactionsJson)
                .First(t => t.User.Id == TESTING_USER_ID && t.Category.Id == category.Id);

            transaction.User = user;
            transaction.Category = category;
            transaction.Wallet = wallet;

            double initBalance = wallet.Balance;

            WalletRepository.Setup(w => w.Update(wallet)).ReturnsAsync(wallet);


            var resultWallet = await WalletService.UpdateBalance(transaction, transaction, isDelete: true);


            Assert.NotNull(resultWallet);
            Assert.NotNull(resultWallet.Currency);
            Assert.Equal(wallet.Currency.Abbreviation, resultWallet.Currency.Abbreviation);
            Assert.Equal(wallet.Name, resultWallet.Name);
            Assert.Equal(wallet.IsDefault, resultWallet.IsDefault);

            if (category.Type.Name == TransactionType.Options.Income.ToString())
            {
                Assert.Equal(initBalance - transaction.Amount, resultWallet.Balance);
            }
            else if (category.Type.Name == TransactionType.Options.Expense.ToString())
            {
                Assert.Equal(initBalance + transaction.Amount, resultWallet.Balance);
            }
        }

        [Fact]
        public async Task CreateTransfer_ReturnsNewTransfer()
        {
            string walletsJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"Wallets.json");
            var wallets = JsonConvert.DeserializeObject<List<Wallet>>(walletsJson).Where(w => w.User.Id == TESTING_USER_ID);

            const string sourceCurrencyAbbreviation = "USD";
            const string targetCurrencyAbbreviation = "BYN";
            const double shareAmount = 1;
            const float exchangeRate = 2.61f;

            var sourceWallet = wallets.First(s => s.Currency.Abbreviation == sourceCurrencyAbbreviation);
            var targetWallet = wallets.First(s => s.Currency.Abbreviation == targetCurrencyAbbreviation);

            double initSourceBalance = sourceWallet.Balance;
            double initTargetBalance = targetWallet.Balance;

            var transfer = new Transfer
            {
                SourceWalletId = sourceWallet.Id,
                TargetWalletId = targetWallet.Id,
                Amount = shareAmount,
                ExchangeRate = exchangeRate
            };

            WalletRepository
                .Setup(w => w.GetByKey(sourceWallet.Id))
                .Returns(sourceWallet);

            WalletRepository
                .Setup(w => w.GetByKey(targetWallet.Id))
                .Returns(targetWallet);

            WalletRepository.Setup(w => w.Update(sourceWallet)).ReturnsAsync(sourceWallet);

            WalletRepository.Setup(w => w.Update(targetWallet)).ReturnsAsync(targetWallet);


            var resultTransfer = await WalletService.CreateTransfer(transfer);


            Assert.NotNull(resultTransfer);
            Assert.NotNull(resultTransfer.SourceWallet);
            Assert.NotNull(resultTransfer.TargetWallet);
            Assert.Equal(sourceWallet.Id, resultTransfer.SourceWallet.Id);
            Assert.Equal(targetWallet.Id, resultTransfer.TargetWallet.Id);
            Assert.Equal(shareAmount, resultTransfer.Amount);
            Assert.Equal(exchangeRate, resultTransfer.ExchangeRate);
            Assert.Equal(initSourceBalance - shareAmount, resultTransfer.SourceWallet.Balance);
            Assert.Equal(initTargetBalance + shareAmount * exchangeRate, resultTransfer.TargetWallet.Balance);
        }

        [Fact]
        public async Task CreateTransfer_ThrowsErrorAboutMoney()
        {
            string walletsJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"Wallets.json");
            var wallets = JsonConvert.DeserializeObject<List<Wallet>>(walletsJson).Where(w => w.User.Id == TESTING_USER_ID);

            const string sourceCurrencyAbbreviation = "USD";
            const string targetCurrencyAbbreviation = "BYN";
            const double shareAmount = 10000000;
            const float exchangeRate = 2.61f;

            var sourceWallet = wallets.First(s => s.Currency.Abbreviation == sourceCurrencyAbbreviation);
            var targetWallet = wallets.First(s => s.Currency.Abbreviation == targetCurrencyAbbreviation);

            var transfer = new Transfer
            {
                SourceWalletId = sourceWallet.Id,
                TargetWalletId = targetWallet.Id,
                Amount = shareAmount,
                ExchangeRate = exchangeRate
            };

            WalletRepository
                .Setup(w => w.GetByKey(sourceWallet.Id))
                .Returns(sourceWallet);

            WalletRepository
                .Setup(w => w.GetByKey(targetWallet.Id))
                .Returns(targetWallet);

            WalletRepository.Setup(w => w.Update(sourceWallet)).ReturnsAsync(sourceWallet);

            WalletRepository.Setup(w => w.Update(targetWallet)).ReturnsAsync(targetWallet);


            await Assert.ThrowsAsync<CashSchedulerException>(async () =>
            {
                await WalletService.CreateTransfer(transfer);
            });
        }

        [Fact]
        public async Task Delete_ReturnsDeletedWallet()
        {   
            string walletsJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"Wallets.json");
            var wallet = JsonConvert.DeserializeObject<List<Wallet>>(walletsJson)
                .First(w => w.User.Id == TESTING_USER_ID && !w.IsDefault);

            WalletRepository.Setup(w => w.GetByKey(wallet.Id)).Returns(wallet);

            WalletRepository.Setup(w => w.Delete(wallet.Id)).ReturnsAsync(wallet);


            var resultWallet = await WalletService.Delete(wallet.Id);


            Assert.NotNull(resultWallet);
            Assert.Equal(wallet.Name, resultWallet.Name);
            Assert.Equal(wallet.Balance, resultWallet.Balance);
            Assert.Equal(wallet.IsDefault, resultWallet.IsDefault);
        }

        [Fact]
        public async Task Delete_ThrowsErrorAboutDefaultWallet()
        {
            string walletsJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"Wallets.json");
            var wallet = JsonConvert.DeserializeObject<List<Wallet>>(walletsJson)
                .First(w => w.User.Id == TESTING_USER_ID && w.IsDefault);

            WalletRepository.Setup(w => w.GetByKey(wallet.Id)).Returns(wallet);

            await Assert.ThrowsAsync<CashSchedulerException>(async () =>
            {
                await WalletService.Delete(wallet.Id);
            });
        }
    }
}
