using System.Collections.Generic;
using CashSchedulerWebServer.Models;

namespace CashSchedulerWebServer.Services.Contracts
{
    public interface ICurrencyService : IService<string, Currency>
    {
        IEnumerable<Currency> GetAll();
        Currency GetDefaultCurrency();
    }
}
