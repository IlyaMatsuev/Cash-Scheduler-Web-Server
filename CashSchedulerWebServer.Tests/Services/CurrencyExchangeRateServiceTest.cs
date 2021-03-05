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
using CashSchedulerWebServer.Services.Currencies;
using Moq;
using Newtonsoft.Json;
using Xunit;

namespace CashSchedulerWebServer.Tests.Services
{
    public class CurrencyExchangeRateServiceTest
    {
        private const int TESTING_USER_ID = 1;
        private ICurrencyExchangeRateService CurrencyExchangeRateService { get; }
        private Mock<ICurrencyExchangeRateRepository> CurrencyExchangeRateRepository { get; }
        private Mock<ICurrencyRepository> CurrencyRepository { get; }
        private Mock<IUserRepository> UserRepository { get; }
        private Mock<IContextProvider> ContextProvider { get; }
        private Mock<IUserContext> UserContext { get; }
        
        public CurrencyExchangeRateServiceTest()
        {
            ContextProvider = new Mock<IContextProvider>();
            CurrencyExchangeRateRepository = new Mock<ICurrencyExchangeRateRepository>();
            CurrencyRepository = new Mock<ICurrencyRepository>();
            UserRepository = new Mock<IUserRepository>();
            UserContext = new Mock<IUserContext>();

            UserContext.Setup(c => c.GetUserId()).Returns(TESTING_USER_ID);

            ContextProvider
                .Setup(c => c.GetRepository<ICurrencyExchangeRateRepository>())
                .Returns(CurrencyExchangeRateRepository.Object);
            
            ContextProvider
                .Setup(c => c.GetRepository<ICurrencyRepository>())
                .Returns(CurrencyRepository.Object);
            
            ContextProvider
                .Setup(c => c.GetRepository<IUserRepository>())
                .Returns(UserRepository.Object);

            //CurrencyExchangeRateService = new CurrencyExchangeRateService(ContextProvider.Object, UserContext.Object);
        }


        [Fact]
        public async Task Create_ReturnsNewExchangeRate()
        {
            string usersJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"Users.json");
            var user = JsonConvert.DeserializeObject<List<User>>(usersJson).First(u => u.Id == TESTING_USER_ID);
            
            string currenciesJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"Currencies.json");
            var currencies = JsonConvert.DeserializeObject<List<Currency>>(currenciesJson);

            const string sourceCurrencyAbbreviation = "EUR";
            const string targetCurrencyAbbreviation = "USD";
            const float exchangeRate = 1.12f;

            var sourceCurrency = currencies.First(c => c.Abbreviation == sourceCurrencyAbbreviation);
            var targetCurrency = currencies.First(c => c.Abbreviation == targetCurrencyAbbreviation);

            var newExchangeRate = new CurrencyExchangeRate
            {
                SourceCurrencyAbbreviation = sourceCurrencyAbbreviation,
                TargetCurrencyAbbreviation = targetCurrencyAbbreviation,
                ExchangeRate = exchangeRate,
                IsCustom = true
            };

            UserRepository.Setup(u => u.GetByKey(TESTING_USER_ID)).Returns(user);
            
            CurrencyRepository.Setup(c => c.GetByKey(sourceCurrencyAbbreviation)).Returns(sourceCurrency);
            CurrencyRepository.Setup(c => c.GetByKey(targetCurrencyAbbreviation)).Returns(targetCurrency);

            CurrencyExchangeRateRepository.Setup(c => c.Create(newExchangeRate)).ReturnsAsync(newExchangeRate);


            var resultExchangeRate = await CurrencyExchangeRateService.Create(newExchangeRate);


            Assert.NotNull(resultExchangeRate);
            Assert.NotNull(resultExchangeRate.SourceCurrency);
            Assert.NotNull(resultExchangeRate.TargetCurrency);
            Assert.NotNull(resultExchangeRate.User);
            Assert.Equal(TESTING_USER_ID, resultExchangeRate.User.Id);
            Assert.Equal(sourceCurrencyAbbreviation, resultExchangeRate.SourceCurrency.Abbreviation);
            Assert.Equal(targetCurrencyAbbreviation, resultExchangeRate.TargetCurrency.Abbreviation);
            Assert.Equal(exchangeRate, resultExchangeRate.ExchangeRate);
            Assert.Equal(DateTime.Today, resultExchangeRate.ValidFrom);
            Assert.Equal(DateTime.Today, resultExchangeRate.ValidTo);
        }
        
        [Fact]
        public async Task Update_ReturnsUpdatedExchangeRate()
        {
            string usersJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"Users.json");
            var user = JsonConvert.DeserializeObject<List<User>>(usersJson).First(u => u.Id == TESTING_USER_ID);
            
            string currenciesJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"Currencies.json");
            var currencies = JsonConvert.DeserializeObject<List<Currency>>(currenciesJson);

            const int exchangeRateId = 1;
            const string sourceCurrencyAbbreviation = "EUR";
            const string targetCurrencyAbbreviation = "USD";
            const float exchangeRate = 1.12f;

            const string newTargetCurrencyAbbreviation = "RUB";
            const float newExchangeRate = 1.66f;

            var sourceCurrency = currencies.First(c => c.Abbreviation == sourceCurrencyAbbreviation);
            var targetCurrency = currencies.First(c => c.Abbreviation == targetCurrencyAbbreviation);
            var newTargetCurrency = currencies.First(c => c.Abbreviation == newTargetCurrencyAbbreviation);
            
            var oldCurrencyExchangeRate = new CurrencyExchangeRate
            {
                Id = exchangeRateId,
                ExchangeRate = exchangeRate,
                SourceCurrency = sourceCurrency,
                TargetCurrency = targetCurrency,
                ValidFrom = DateTime.Today,
                ValidTo = DateTime.Today.AddDays(1)
            };

            var newCurrencyExchangeRate = new CurrencyExchangeRate
            {
                Id = exchangeRateId,
                TargetCurrencyAbbreviation = newTargetCurrencyAbbreviation,
                ExchangeRate = newExchangeRate,
                ValidTo = DateTime.Today
            };

            UserRepository.Setup(u => u.GetByKey(TESTING_USER_ID)).Returns(user);
            
            CurrencyRepository.Setup(c => c.GetByKey(sourceCurrencyAbbreviation)).Returns(sourceCurrency);
            CurrencyRepository.Setup(c => c.GetByKey(targetCurrencyAbbreviation)).Returns(targetCurrency);
            CurrencyRepository.Setup(c => c.GetByKey(newTargetCurrencyAbbreviation)).Returns(newTargetCurrency);

            CurrencyExchangeRateRepository
                .Setup(c => c.GetByKey(exchangeRateId))
                .Returns(oldCurrencyExchangeRate);
            
            CurrencyExchangeRateRepository
                .Setup(c => c.Update(oldCurrencyExchangeRate))
                .ReturnsAsync(oldCurrencyExchangeRate);


            var resultExchangeRate = await CurrencyExchangeRateService.Update(newCurrencyExchangeRate);


            Assert.NotNull(resultExchangeRate);
            Assert.NotNull(resultExchangeRate.SourceCurrency);
            Assert.NotNull(resultExchangeRate.TargetCurrency);
            Assert.Equal(sourceCurrencyAbbreviation, resultExchangeRate.SourceCurrency.Abbreviation);
            Assert.Equal(newTargetCurrencyAbbreviation, resultExchangeRate.TargetCurrency.Abbreviation);
            Assert.Equal(newExchangeRate, resultExchangeRate.ExchangeRate);
            Assert.Equal(DateTime.Today, resultExchangeRate.ValidFrom);
            Assert.Equal(DateTime.Today, resultExchangeRate.ValidTo);
        }
        
        [Fact]
        public async Task Delete_ReturnsDeletedExchangeRate()
        {
            string currenciesJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"Currencies.json");
            var currencies = JsonConvert.DeserializeObject<List<Currency>>(currenciesJson);
            
            const int exchangeRateId = 1;
            const string sourceCurrencyAbbreviation = "EUR";
            const string targetCurrencyAbbreviation = "USD";
            const float exchangeRate = 1.12f;
            
            var sourceCurrency = currencies.First(c => c.Abbreviation == sourceCurrencyAbbreviation);
            var targetCurrency = currencies.First(c => c.Abbreviation == targetCurrencyAbbreviation);

            var currencyExchangeRate = new CurrencyExchangeRate
            {
                Id = exchangeRateId,
                ExchangeRate = exchangeRate,
                SourceCurrency = sourceCurrency,
                TargetCurrency = targetCurrency,
                IsCustom = true,
                ValidFrom = DateTime.Today,
                ValidTo = DateTime.Today.AddDays(1)
            };

            CurrencyExchangeRateRepository
                .Setup(c => c.GetByKey(exchangeRateId))
                .Returns(currencyExchangeRate);
            
            CurrencyExchangeRateRepository
                .Setup(c => c.Delete(exchangeRateId))
                .ReturnsAsync(currencyExchangeRate);


            var resultExchangeRate = await CurrencyExchangeRateService.Delete(exchangeRateId);


            Assert.NotNull(resultExchangeRate);
        }
        
        [Fact]
        public async Task Delete_ThrowsException()
        {
            string currenciesJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"Currencies.json");
            var currencies = JsonConvert.DeserializeObject<List<Currency>>(currenciesJson);
            
            const int exchangeRateId = 1;
            const string sourceCurrencyAbbreviation = "EUR";
            const string targetCurrencyAbbreviation = "USD";
            const float exchangeRate = 1.12f;
            
            var sourceCurrency = currencies.First(c => c.Abbreviation == sourceCurrencyAbbreviation);
            var targetCurrency = currencies.First(c => c.Abbreviation == targetCurrencyAbbreviation);

            var currencyExchangeRate = new CurrencyExchangeRate
            {
                Id = exchangeRateId,
                ExchangeRate = exchangeRate,
                SourceCurrency = sourceCurrency,
                TargetCurrency = targetCurrency,
                IsCustom = false,
                ValidFrom = DateTime.Today,
                ValidTo = DateTime.Today.AddDays(1)
            };

            CurrencyExchangeRateRepository
                .Setup(c => c.GetByKey(exchangeRateId))
                .Returns(currencyExchangeRate);
            
            CurrencyExchangeRateRepository
                .Setup(c => c.Delete(exchangeRateId))
                .ReturnsAsync(currencyExchangeRate);
            

            await Assert.ThrowsAsync<CashSchedulerException>(async () =>
            {
                await CurrencyExchangeRateService.Delete(exchangeRateId);
            });
        }
    }
}
