using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Exceptions;
using CashSchedulerWebServer.Models;

namespace CashSchedulerWebServer.Db.Repositories
{
    public class CurrencyRepository : ICurrencyRepository
    {
        private CashSchedulerContext Context { get; }
        
        public CurrencyRepository(CashSchedulerContext context)
        {
            Context = context;
        }


        public Currency GetByKey(string abbreviation)
        {
            return Context.Currencies.FirstOrDefault(c => c.Abbreviation == abbreviation);
        }
        
        public Currency GetDefaultCurrency()
        {
            return Context.Currencies.FirstOrDefault(c => c.Abbreviation == Currency.DEFAULT_CURRENCY_ABBREVIATION_USD);
        }

        public IEnumerable<Currency> GetAll()
        {
            return Context.Currencies;
        }

        public Task<Currency> Create(Currency currency)
        {
            throw new CashSchedulerException("It's forbidden to create new currencies");
        }

        public Task<Currency> Update(Currency currency)
        {
            throw new CashSchedulerException("It's forbidden to update the existing currencies");
        }

        public Task<Currency> Delete(string abbreviation)
        {
            throw new CashSchedulerException("It's forbidden to delete the existing currencies");
        }
    }
}
