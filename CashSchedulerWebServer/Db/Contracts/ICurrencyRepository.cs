using CashSchedulerWebServer.Models;

namespace CashSchedulerWebServer.Db.Contracts
{
    public interface ICurrencyRepository : IRepository<string, Currency>
    {
        Currency GetDefaultCurrency();
    }
}
