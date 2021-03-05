using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CashSchedulerWebServer.Auth.Contracts;
using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Exceptions;
using CashSchedulerWebServer.Models;
using CashSchedulerWebServer.Services.Contracts;
using CashSchedulerWebServer.Services.Wallets;
using CashSchedulerWebServer.WebServices.Contracts;
using CashSchedulerWebServer.WebServices.ExchangeRates;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace CashSchedulerWebServer.Tests.Services
{
    public class WalletServiceTest
    {
        /*private const int TESTING_USER_ID = 1;
        
        private IWalletService WalletService { get; }
        private Mock<IContextProvider> ContextProvider { get; }
        private Mock<IUserContext> UserContext { get; }
        private Mock<IWalletRepository> WalletRepository { get; }
        private Mock<IUserRepository> UserRepository { get; }
        private Mock<ICurrencyRepository> CurrencyRepository { get; }
        private Mock<IExchangeRateWebService> ExchangeRateWebService { get; }

        public WalletServiceTest()
        {
            ContextProvider = new Mock<IContextProvider>();
            UserContext = new Mock<IUserContext>();
            WalletRepository = new Mock<IWalletRepository>();
            UserRepository = new Mock<IUserRepository>();
            CurrencyRepository = new Mock<ICurrencyRepository>();
            ExchangeRateWebService = new Mock<IExchangeRateWebService>();

            ContextProvider.Setup(c => c.GetRepository<IWalletRepository>()).Returns(WalletRepository.Object);
            
            ContextProvider.Setup(c => c.GetRepository<IUserRepository>()).Returns(UserRepository.Object);
            
            ContextProvider.Setup(c => c.GetRepository<ICurrencyRepository>()).Returns(CurrencyRepository.Object);

            UserContext.Setup(u => u.GetUserId()).Returns(TESTING_USER_ID);

            WalletService = new WalletService(
                ContextProvider.Object, 
                ExchangeRateWebService.Object, 
                UserContext.Object
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
            const double usdToBynRate = 2.61;
            double initBalance = wallet.Balance;
            double resultBalance = initBalance * usdToBynRate;

            wallet.Currency = currencies.First(c => c.Abbreviation == sourceCurrencyAbbreviation);

            var newWallet = new Wallet
            {
                Id = wallet.Id,
                Name = newName,
                CurrencyAbbreviation = newCurrencyAbbreviation
            };

            ExchangeRateWebService
                .Setup(e => e.ConvertCurrency(sourceCurrencyAbbreviation, newCurrencyAbbreviation, initBalance))
                .ReturnsAsync(new ConvertCurrencyResponse
                {
                    Success = true,
                    Result = resultBalance,
                    Date = DateTime.Today,
                    Historical = false
                });

            CurrencyRepository
                .Setup(c => c.GetByKey(newCurrencyAbbreviation))
                .Returns(currencies.First(cc => cc.Abbreviation == newCurrencyAbbreviation));

            WalletRepository.Setup(w => w.GetByKey(newWallet.Id)).Returns(wallet);
            
            WalletRepository.Setup(w => w.Update(wallet)).ReturnsAsync(wallet);


            var resultWallet = await WalletService.Update(newWallet);


            Assert.NotNull(resultWallet);
            Assert.NotNull(resultWallet.Currency);
            Assert.Equal(newCurrencyAbbreviation, resultWallet.Currency.Abbreviation);
            Assert.Equal(newName, resultWallet.Name);
            Assert.Equal(resultBalance, resultWallet.Balance);
            Assert.Equal(wallet.IsDefault, resultWallet.IsDefault);
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
        }*/
    }
}
