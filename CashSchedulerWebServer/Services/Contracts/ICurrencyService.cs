using CashSchedulerWebServer.Models;

namespace CashSchedulerWebServer.Services.Contracts
{
    public interface ICurrencyService : IService<string, Currency>
    {
        Currency GetDefaultCurrency();
    }
}
