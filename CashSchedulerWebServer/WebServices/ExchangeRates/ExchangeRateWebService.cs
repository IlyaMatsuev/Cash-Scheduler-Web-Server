using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using CashSchedulerWebServer.WebServices.Contracts;
using Microsoft.Extensions.Configuration;
using Tiny.RestClient;

namespace CashSchedulerWebServer.WebServices.ExchangeRates
{
    public class ExchangeRateWebService : IExchangeRateWebService
    {
        private TinyRestClient Client { get; }

        public ExchangeRateWebService(IConfiguration configuration)
        {
            Client = new TinyRestClient(new HttpClient(), configuration["WebServices:CurrencyExchangeRates:Endpoint"]);
        }
        
        
        public Task<ExchangeRatesResponse> GetLatestExchangeRates(string sourceCurrency)
        {
            return Client.GetRequest("latest")
                .AddQueryParameter("base", sourceCurrency)
                .ExecuteAsync<ExchangeRatesResponse>();
        }
        
        public Task<ExchangeRatesResponse> GetLatestExchangeRates(string sourceCurrency, IEnumerable<string> targetCurrencies)
        {
            return Client.GetRequest("latest")
                .AddQueryParameter("base", sourceCurrency)
                .AddQueryParameter("symbols", string.Join(',', targetCurrencies))
                .AddQueryParameter("places", 2)
                .ExecuteAsync<ExchangeRatesResponse>();
        }

        public Task<ConvertCurrencyResponse> ConvertCurrency(string sourceCurrency, string targetCurrency, double amount = 1)
        {
            return Client.GetRequest("convert")
                .AddQueryParameter("from", sourceCurrency)
                .AddQueryParameter("to", targetCurrency)
                .AddQueryParameter("amount", amount)
                .AddQueryParameter("places", 2)
                .ExecuteAsync<ConvertCurrencyResponse>();
        }
    }
}
