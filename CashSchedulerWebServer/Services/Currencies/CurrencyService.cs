using System.Collections.Generic;
using System.Threading.Tasks;
using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Models;
using CashSchedulerWebServer.Services.Contracts;

namespace CashSchedulerWebServer.Services.Currencies
{
    public class CurrencyService : ICurrencyService
    {
        private ICurrencyRepository CurrencyRepository { get; }
        
        public CurrencyService(IContextProvider contextProvider)
        {
            CurrencyRepository = contextProvider.GetRepository<ICurrencyRepository>();
        }


        public IEnumerable<Currency> GetAll()
        {
            return CurrencyRepository.GetAll();
        }
        
        public Currency GetDefaultCurrency()
        {
            return CurrencyRepository.GetDefaultCurrency();
        }
        
        public Task<Currency> Create(Currency currency)
        {
            return CurrencyRepository.Create(currency);
        }

        public Task<Currency> Update(Currency currency)
        {
            return CurrencyRepository.Update(currency);
        }

        public Task<Currency> Delete(string abbreviation)
        {
            return CurrencyRepository.Delete(abbreviation);
        }
    }
}
