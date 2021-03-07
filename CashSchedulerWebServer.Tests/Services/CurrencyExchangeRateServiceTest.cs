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
using CashSchedulerWebServer.WebServices.Contracts;
using CashSchedulerWebServer.WebServices.ExchangeRates;
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
        private Mock<IExchangeRateWebService> ExchangeRateWebService { get; }
        
        public CurrencyExchangeRateServiceTest()
        {
            ContextProvider = new Mock<IContextProvider>();
            CurrencyExchangeRateRepository = new Mock<ICurrencyExchangeRateRepository>();
            CurrencyRepository = new Mock<ICurrencyRepository>();
            UserRepository = new Mock<IUserRepository>();
            UserContext = new Mock<IUserContext>();
            ExchangeRateWebService = new Mock<IExchangeRateWebService>();

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

            CurrencyExchangeRateService = new CurrencyExchangeRateService(
                ContextProvider.Object,
                UserContext.Object,
                ExchangeRateWebService.Object
            );
        }


        [Fact]
        public async Task GetBySourceAndTarget()
        {
            string currenciesJson = File.ReadAllText(TestConfiguration.MockDataFolderPath + @"Currencies.json");
            var currencies = JsonConvert.DeserializeObject<List<Currency>>(currenciesJson);


            const string sourceCurrencyAbbreviation = "USD";
            const string targetCurrencyAbbreviation = "BYN";
            const float exchangeRate = 2.61f;

            ExchangeRateWebService
                .Setup(e => e.GetLatestExchangeRates(sourceCurrencyAbbreviation, new[] {targetCurrencyAbbreviation}))
                .ReturnsAsync(new ExchangeRatesResponse
                {
                    Base = sourceCurrencyAbbreviation,
                    Date = DateTime.Today,
                    Success = true,
                    Rates = new Dictionary<string, float>
                    {
                        {targetCurrencyAbbreviation, exchangeRate}
                    }
                });

            CurrencyRepository
                .Setup(c => c.GetByKey(sourceCurrencyAbbreviation))
                .Returns(currencies.First(cur => cur.Abbreviation == sourceCurrencyAbbreviation));

            CurrencyRepository
                .Setup(c => c.GetByKey(targetCurrencyAbbreviation))
                .Returns(currencies.First(cur => cur.Abbreviation == targetCurrencyAbbreviation));

            CurrencyExchangeRateRepository
                .Setup(c => c.GetBySourceAndTarget(sourceCurrencyAbbreviation, targetCurrencyAbbreviation))
                .Returns(new List<CurrencyExchangeRate>());


            var resultExchangeRates = await CurrencyExchangeRateService.GetBySourceAndTarget(
                sourceCurrencyAbbreviation, targetCurrencyAbbreviation
            );


            Assert.NotNull(resultExchangeRates);
            Assert.Single(resultExchangeRates);
            Assert.NotNull(resultExchangeRates.First().SourceCurrency);
            Assert.NotNull(resultExchangeRates.First().TargetCurrency);
            Assert.Equal(sourceCurrencyAbbreviation, resultExchangeRates.First().SourceCurrency.Abbreviation);
            Assert.Equal(targetCurrencyAbbreviation, resultExchangeRates.First().TargetCurrency.Abbreviation);
            Assert.Equal(exchangeRate, resultExchangeRates.First().ExchangeRate);
            Assert.False(resultExchangeRates.First().IsCustom);
            Assert.Equal(DateTime.Today, resultExchangeRates.First().ValidFrom);
            Assert.Equal(DateTime.Today, resultExchangeRates.First().ValidTo);

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
