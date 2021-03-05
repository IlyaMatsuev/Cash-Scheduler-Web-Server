using System.Collections.Generic;
using System.Threading.Tasks;
using CashSchedulerWebServer.WebServices.ExchangeRates;

namespace CashSchedulerWebServer.WebServices.Contracts
{
    public interface IExchangeRateWebService
    {
        Task<ExchangeRatesResponse> GetLatestExchangeRates(string sourceCurrency);
        Task<ExchangeRatesResponse> GetLatestExchangeRates(string sourceCurrency, IEnumerable<string> targetCurrencies);
        Task<ConvertCurrencyResponse> ConvertCurrency(string sourceCurrency, string targetCurrency, double amount = 1);
    }
}
