using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CashSchedulerWebServer.Db;
using CashSchedulerWebServer.Exceptions;
using CashSchedulerWebServer.Models;
using CashSchedulerWebServer.WebServices.Contracts;
using CashSchedulerWebServer.WebServices.ExchangeRates;
using Quartz;

namespace CashSchedulerWebServer.Jobs.ExchangeRates
{
    public class ExchangeRateJob : IJob
    {
        private CashSchedulerContext Context { get; }
        private IExchangeRateService ExchangeRateService { get; }

        public ExchangeRateJob(CashSchedulerContext context, IExchangeRateService exchangeRateService)
        {
            Context = context;
            ExchangeRateService = exchangeRateService;
        }
        
        
        public async Task Execute(IJobExecutionContext context)
        {
            Console.WriteLine($"Running the {context.JobDetail.Description}");
            
            await using var dmlTransaction = await Context.Database.BeginTransactionAsync();
            try
            {
                int freshExchangeRatesCount = 0;
                foreach (string sourceCurrency in GetGlobalCurrencies())
                {
                    var exchangeRates = await ExchangeRateService.GetLatestExchangeRates(sourceCurrency);

                    if (exchangeRates.Success)
                    {
                        await Context.CurrencyExchangeRates.AddRangeAsync(ParseExchangeRates(exchangeRates));
                        freshExchangeRatesCount += await Context.SaveChangesAsync();
                    }
                }
                
                await dmlTransaction.CommitAsync();

                Console.WriteLine($"{freshExchangeRatesCount} exchange rates were updated");
            }
            catch (Exception error)
            {
                dmlTransaction.Rollback();
                Console.WriteLine($"Error while running the {context.JobDetail.Description}: {error.Message}: \n{error.StackTrace}");
            }
        }


        private string[] GetGlobalCurrencies()
        {
            return new[]
            {
                Currency.DEFAULT_CURRENCY_ABBREVIATION_USD, 
                Currency.DEFAULT_CURRENCY_ABBREVIATION_EUR
            };
        }

        private IEnumerable<CurrencyExchangeRate> ParseExchangeRates(ExchangeRatesResponse exchangeRatesResponse)
        {
            var currencies = Context.Currencies.ToList();

            var sourceCurrency = currencies.FirstOrDefault(c => c.Abbreviation == exchangeRatesResponse.Base);

            if (sourceCurrency == null)
            {
                throw GetNoCurrencyException(exchangeRatesResponse.Base);
            }
            
            return exchangeRatesResponse.Rates.Select(rate => new CurrencyExchangeRate
            {
                SourceCurrency = sourceCurrency,
                TargetCurrency = currencies.FirstOrDefault(c => c.Abbreviation == rate.Key) 
                                 ?? throw GetNoCurrencyException(rate.Key),
                ExchangeRate = rate.Value,
                ValidFrom = DateTime.Today,
                ValidTo = DateTime.Today.AddDays(1),
                IsCustom = false
            });
        }

        private Exception GetNoCurrencyException(string currency)
        {
            return new CashSchedulerException($"There is no such currency: {currency}");
        }
    }
}
