using System.Collections.Generic;
using System.Threading.Tasks;
using CashSchedulerWebServer.Models;

namespace CashSchedulerWebServer.Db.Contracts
{
    public interface ICurrencyExchangeRateRepository : IRepository<int, CurrencyExchangeRate>
    {
        IEnumerable<CurrencyExchangeRate> GetBySourceAndTarget(
            string sourceCurrencyAbbreviation,
            string targetCurrencyAbbreviation
        );

        IEnumerable<CurrencyExchangeRate> DeleteByUserId(int userId);
    }
}
