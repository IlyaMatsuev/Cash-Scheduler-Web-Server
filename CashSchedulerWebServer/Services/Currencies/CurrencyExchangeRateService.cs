﻿using System.Collections.Generic;
using System.Threading.Tasks;
using CashSchedulerWebServer.Auth.Contracts;
using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Exceptions;
using CashSchedulerWebServer.Models;
using CashSchedulerWebServer.Services.Contracts;

namespace CashSchedulerWebServer.Services.Currencies
{
    public class CurrencyExchangeRateService : ICurrencyExchangeRateService
    {
        private IContextProvider ContextProvider { get; }
        
        private int UserId { get; }

        public CurrencyExchangeRateService(IContextProvider contextProvider, IUserContext userContext)
        {
            ContextProvider = contextProvider;
            UserId = userContext.GetUserId();
        }
        
        
        public IEnumerable<CurrencyExchangeRate> GetAll()
        {
            return ContextProvider.GetRepository<ICurrencyExchangeRateRepository>().GetAll();
        }
        
        public Task<CurrencyExchangeRate> Create(CurrencyExchangeRate exchangeRate)
        {
            var currencyRepository = ContextProvider.GetRepository<ICurrencyRepository>();

            exchangeRate.SourceCurrency ??= currencyRepository.GetById(exchangeRate.SourceCurrencyAbbreviation);
            exchangeRate.TargetCurrency ??= currencyRepository.GetById(exchangeRate.TargetCurrencyAbbreviation);

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
                exchangeRate.User = ContextProvider.GetRepository<IUserRepository>().GetById(UserId);                
            }
            
            return ContextProvider.GetRepository<ICurrencyExchangeRateRepository>().Create(exchangeRate);
        }

        public Task<CurrencyExchangeRate> Update(CurrencyExchangeRate exchangeRate)
        {
            var exchangeRateRepository = ContextProvider.GetRepository<ICurrencyExchangeRateRepository>();
            
            var targetExchangeRate = exchangeRateRepository.GetById(exchangeRate.Id);
            if (targetExchangeRate == null)
            {
                throw new CashSchedulerException("There is no such exchange rate");
            }

            var currencyRepository = ContextProvider.GetRepository<ICurrencyRepository>();

            if (!string.IsNullOrEmpty(exchangeRate.SourceCurrencyAbbreviation))
            {
                targetExchangeRate.SourceCurrency = currencyRepository.GetById(exchangeRate.SourceCurrencyAbbreviation);
                
                if (targetExchangeRate.SourceCurrency == null)
                {
                    throw new CashSchedulerException("There is no such currency", new[] { "sourceCurrencyAbbreviation" });
                }
            }
            
            if (!string.IsNullOrEmpty(exchangeRate.TargetCurrencyAbbreviation))
            {
                targetExchangeRate.TargetCurrency = currencyRepository.GetById(exchangeRate.TargetCurrencyAbbreviation);
                
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

            var exchangeRate = exchangeRateRepository.GetById(id);
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
