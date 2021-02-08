using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CashSchedulerWebServer.Auth.Contracts;
using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Models;
using CashSchedulerWebServer.Utils;
using Microsoft.EntityFrameworkCore;

namespace CashSchedulerWebServer.Db.Repositories
{
    public class CurrencyExchangeRateRepository : ICurrencyExchangeRateRepository
    {
        private CashSchedulerContext Context { get; }
        private int UserId { get; }
        
        public CurrencyExchangeRateRepository(CashSchedulerContext context, IUserContext userContext)
        {
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
            ModelValidator.ValidateModelAttributes(exchangeRate);

            await Context.CurrencyExchangeRates.AddAsync(exchangeRate);
            await Context.SaveChangesAsync();
            
            return GetById(exchangeRate.Id);
        }

        public async Task<CurrencyExchangeRate> Update(CurrencyExchangeRate exchangeRate)
        {
            ModelValidator.ValidateModelAttributes(exchangeRate);

            Context.CurrencyExchangeRates.Update(exchangeRate);
            await Context.SaveChangesAsync();

            return GetById(exchangeRate.Id);
        }

        public async Task<CurrencyExchangeRate> Delete(int id)
        {
            var exchangeRate = GetById(id);
            
            Context.CurrencyExchangeRates.Remove(exchangeRate);
            await Context.SaveChangesAsync();

            return exchangeRate;
        }
    }
}
