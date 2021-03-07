using System.Collections.Generic;
using System.Threading.Tasks;
using CashSchedulerWebServer.Auth.Contracts;
using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Exceptions;
using CashSchedulerWebServer.Models;
using CashSchedulerWebServer.Services.Contracts;
using CashSchedulerWebServer.WebServices.Contracts;

namespace CashSchedulerWebServer.Services.Currencies
{
    public class CurrencyExchangeRateService : ICurrencyExchangeRateService
    {
        private IContextProvider ContextProvider { get; }
        private IExchangeRateWebService ExchangeRateWebService { get; }
        
        private int UserId { get; }

        public CurrencyExchangeRateService(
            IContextProvider contextProvider,
            IUserContext userContext,
            IExchangeRateWebService exchangeRateWebService
        )
        {
            ContextProvider = contextProvider;
            UserId = userContext.GetUserId();
            ExchangeRateWebService = exchangeRateWebService;
        }
        
        
        public IEnumerable<CurrencyExchangeRate> GetAll()
        {
            return ContextProvider.GetRepository<ICurrencyExchangeRateRepository>().GetAll();
        }

        public async Task<IEnumerable<CurrencyExchangeRate>> GetBySourceAndTarget(
            string sourceCurrencyAbbreviation,
            string targetCurrencyAbbreviation
        )
        {
            var rates = new List<CurrencyExchangeRate>();
            var exchangeRatesResponse = await ExchangeRateWebService.GetLatestExchangeRates(
                sourceCurrencyAbbreviation,
                new[] {targetCurrencyAbbreviation}
            );

            if (exchangeRatesResponse.Success)
            {
                var currencyRepository = ContextProvider.GetRepository<ICurrencyRepository>();
                rates.Add(new CurrencyExchangeRate
                {
                    ExchangeRate = exchangeRatesResponse.Rates[targetCurrencyAbbreviation],
                    IsCustom = false,
                    SourceCurrency = currencyRepository.GetByKey(sourceCurrencyAbbreviation),
                    TargetCurrency = currencyRepository.GetByKey(targetCurrencyAbbreviation),
                    ValidFrom = exchangeRatesResponse.Date,
                    ValidTo = exchangeRatesResponse.Date
                });
            }

            rates.AddRange(
                ContextProvider.GetRepository<ICurrencyExchangeRateRepository>()
                    .GetBySourceAndTarget(sourceCurrencyAbbreviation, targetCurrencyAbbreviation)
            );

            return rates;
        }

        public Task<CurrencyExchangeRate> Create(CurrencyExchangeRate exchangeRate)
        {
            var currencyRepository = ContextProvider.GetRepository<ICurrencyRepository>();

            exchangeRate.SourceCurrency ??= currencyRepository.GetByKey(exchangeRate.SourceCurrencyAbbreviation);
            exchangeRate.TargetCurrency ??= currencyRepository.GetByKey(exchangeRate.TargetCurrencyAbbreviation);

            if (exchangeRate.SourceCurrency == null)
            {
                throw new CashSchedulerException("There is no such currency", new[] { "sourceCurrencyAbbreviation" });
            }
            
            if (exchangeRate.TargetCurrency == null)
            {
                throw new CashSchedulerException("There is no such currency", new[] { "targetCurrencyAbbreviation" });
            }

            if (exchangeRate.IsCustom && exchangeRate.User == null)
            {
                exchangeRate.User = ContextProvider.GetRepository<IUserRepository>().GetByKey(UserId);                
            }

            return ContextProvider.GetRepository<ICurrencyExchangeRateRepository>().Create(exchangeRate);
        }

        public Task<CurrencyExchangeRate> Update(CurrencyExchangeRate exchangeRate)
        {
            var exchangeRateRepository = ContextProvider.GetRepository<ICurrencyExchangeRateRepository>();
            
            var targetExchangeRate = exchangeRateRepository.GetByKey(exchangeRate.Id);
            if (targetExchangeRate == null)
            {
                throw new CashSchedulerException("There is no such exchange rate");
            }

            var currencyRepository = ContextProvider.GetRepository<ICurrencyRepository>();

            if (!string.IsNullOrEmpty(exchangeRate.SourceCurrencyAbbreviation))
            {
                targetExchangeRate.SourceCurrency = currencyRepository.GetByKey(exchangeRate.SourceCurrencyAbbreviation);
                
                if (targetExchangeRate.SourceCurrency == null)
                {
                    throw new CashSchedulerException("There is no such currency", new[] { "sourceCurrencyAbbreviation" });
                }
            }
            
            if (!string.IsNullOrEmpty(exchangeRate.TargetCurrencyAbbreviation))
            {
                targetExchangeRate.TargetCurrency = currencyRepository.GetByKey(exchangeRate.TargetCurrencyAbbreviation);
                
                if (targetExchangeRate.TargetCurrency == null)
                {
                    throw new CashSchedulerException("There is no such currency", new[] { "targetCurrencyAbbreviation" });
                }
            }

            if (exchangeRate.ExchangeRate != default)
            {
                targetExchangeRate.ExchangeRate = exchangeRate.ExchangeRate;
            }
            
            if (exchangeRate.ValidFrom != default)
            {
                targetExchangeRate.ValidFrom = exchangeRate.ValidFrom;
            }
            
            if (exchangeRate.ValidTo != default)
            {
                targetExchangeRate.ValidTo = exchangeRate.ValidTo;
            }
            
            return exchangeRateRepository.Update(targetExchangeRate);
        }

        public Task<CurrencyExchangeRate> Delete(int id)
        {
            var exchangeRateRepository = ContextProvider.GetRepository<ICurrencyExchangeRateRepository>();

            var exchangeRate = exchangeRateRepository.GetByKey(id);
            if (exchangeRate == null)
            {
                throw new CashSchedulerException("There is no exchange rate with such id");
            }

            if (!exchangeRate.IsCustom)
            {
                throw new CashSchedulerException("You cannot delete one of the standard exchange rates");
            }

            return exchangeRateRepository.Delete(id);
        }
    }
}
