using System.Collections.Generic;
using CashSchedulerWebServer.Models;

namespace CashSchedulerWebServer.Services.Contracts
{
    public interface ICurrencyExchangeRateService : IService<int, CurrencyExchangeRate>
    {
        IEnumerable<CurrencyExchangeRate> GetAll();
    }
}
