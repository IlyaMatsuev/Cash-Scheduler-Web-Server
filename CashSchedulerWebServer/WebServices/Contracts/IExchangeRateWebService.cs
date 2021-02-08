using System.Threading.Tasks;
using CashSchedulerWebServer.WebServices.ExchangeRates;

namespace CashSchedulerWebServer.WebServices.Contracts
{
    public interface IExchangeRateWebService
    {
        Task<ExchangeRatesResponse> GetLatestExchangeRates(string sourceCurrency);
        Task<ConvertCurrencyResponse> ConvertCurrency(string sourceCurrency, string targetCurrency, double amount = 1);
    }
}
