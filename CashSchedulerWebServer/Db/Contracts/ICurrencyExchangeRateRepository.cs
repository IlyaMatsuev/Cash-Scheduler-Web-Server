using System.Collections.Generic;
using CashSchedulerWebServer.Models;

namespace CashSchedulerWebServer.Db.Contracts
{
    public interface ICurrencyExchangeRateRepository : IRepository<int, CurrencyExchangeRate>
    {
        IEnumerable<CurrencyExchangeRate> GetBySourceAndTarget(
            string sourceCurrencyAbbreviation,
            string targetCurrencyAbbreviation
        );
    }
}
