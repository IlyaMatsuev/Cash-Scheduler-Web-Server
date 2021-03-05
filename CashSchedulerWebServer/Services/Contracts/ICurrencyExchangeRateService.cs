using System.Collections.Generic;
using System.Threading.Tasks;
using CashSchedulerWebServer.Models;

namespace CashSchedulerWebServer.Services.Contracts
{
    public interface ICurrencyExchangeRateService : IService<int, CurrencyExchangeRate>
    {
        IEnumerable<CurrencyExchangeRate> GetAll();
        Task<IEnumerable<CurrencyExchangeRate>> GetBySourceAndTarget(
            string sourceCurrencyAbbreviation,
            string targetCurrencyAbbreviation
        );
    }
}
