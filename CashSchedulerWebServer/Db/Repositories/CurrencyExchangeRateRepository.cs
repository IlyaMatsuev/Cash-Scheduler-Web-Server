using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CashSchedulerWebServer.Auth.Contracts;
using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Exceptions;
using CashSchedulerWebServer.Models;
using CashSchedulerWebServer.Utils;
using Microsoft.EntityFrameworkCore;

namespace CashSchedulerWebServer.Db.Repositories
{
    public class CurrencyExchangeRateRepository : ICurrencyExchangeRateRepository
    {
        private IContextProvider ContextProvider { get; }
        private CashSchedulerContext Context { get; }
        private int UserId { get; }
        
        public CurrencyExchangeRateRepository(
            CashSchedulerContext context,
            IUserContext userContext,
            IContextProvider contextProvider)
        {
            ContextProvider = contextProvider;
            Context = context;
            UserId = userContext.GetUserId();
        }
        
        
        public IEnumerable<CurrencyExchangeRate> GetAll()
        {
            return Context.CurrencyExchangeRates
                .Where(r => (r.User != null && r.User.Id == UserId && r.IsCustom) || !r.IsCustom)
                .Include(r => r.SourceCurrency)
                .Include(r => r.TargetCurrency)
                .Include(r => r.User);
        }

        public CurrencyExchangeRate GetById(int id)
        {
            return Context.CurrencyExchangeRates.
                Where(r => r.Id == id && ((r.User != null && r.User.Id == UserId && r.IsCustom) || !r.IsCustom))
                .Include(r => r.SourceCurrency)
                .Include(r => r.TargetCurrency)
                .Include(r => r.User)
                .FirstOrDefault();
        }

        public async Task<CurrencyExchangeRate> Create(CurrencyExchangeRate exchangeRate)
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

            ModelValidator.ValidateModelAttributes(exchangeRate);

            Context.CurrencyExchangeRates.Add(exchangeRate);
            await Context.SaveChangesAsync();

            return exchangeRate;
        }

        public async Task<CurrencyExchangeRate> Update(CurrencyExchangeRate exchangeRate)
        {
            var targetExchangeRate = GetById(exchangeRate.Id);
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

            ModelValidator.ValidateModelAttributes(targetExchangeRate);

            Context.CurrencyExchangeRates.Update(targetExchangeRate);
            await Context.SaveChangesAsync();

            return targetExchangeRate;
        }

        public async Task<CurrencyExchangeRate> Delete(int id)
        {
            var exchangeRate = GetById(id);
            if (exchangeRate == null)
            {
                throw new CashSchedulerException("There is no exchange rate with such id");
            }

            if (!exchangeRate.IsCustom)
            {
                throw new CashSchedulerException("You cannot delete one of the standard exchange rates");
            }

            Context.CurrencyExchangeRates.Remove(exchangeRate);
            await Context.SaveChangesAsync();

            return exchangeRate;
        }
    }
}
