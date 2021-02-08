using System.Threading.Tasks;
using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Models;
using CashSchedulerWebServer.Services.Contracts;

namespace CashSchedulerWebServer.Services.Currencies
{
    public class CurrencyService : ICurrencyService
    {
        private IContextProvider ContextProvider { get; }
        
        public CurrencyService(IContextProvider contextProvider)
        {
            ContextProvider = contextProvider;
        }
        
        
        public Currency GetDefaultCurrency()
        {
            return ContextProvider.GetRepository<ICurrencyRepository>().GetDefaultCurrency();
        }
        
        public Task<Currency> Create(Currency currency)
        {
            return ContextProvider.GetRepository<ICurrencyRepository>().Create(currency);
        }

        public Task<Currency> Update(Currency currency)
        {
            return ContextProvider.GetRepository<ICurrencyRepository>().Update(currency);
        }

        public Task<Currency> Delete(string abbreviation)
        {
            return ContextProvider.GetRepository<ICurrencyRepository>().Delete(abbreviation);
        }
    }
}
