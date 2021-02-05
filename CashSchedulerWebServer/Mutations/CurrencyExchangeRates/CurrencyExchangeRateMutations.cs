using System;
using System.Threading.Tasks;
using CashSchedulerWebServer.Auth;
using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Models;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Types;

namespace CashSchedulerWebServer.Mutations.CurrencyExchangeRates
{
    [ExtendObjectType(Name = "Mutation")]
    public class CurrencyExchangeRateMutations
    {
        [GraphQLNonNullType]
        [Authorize(Policy = AuthOptions.AUTH_POLICY)]
        public Task<CurrencyExchangeRate> CreateExchangeRate(
            [Service] IContextProvider contextProvider,
            [GraphQLNonNullType] NewExchangeRateInput exchangeRate)
        {
            return contextProvider.GetRepository<ICurrencyExchangeRateRepository>().Create(new CurrencyExchangeRate
            {
                SourceCurrencyAbbreviation = exchangeRate.SourceCurrencyAbbreviation,
                TargetCurrencyAbbreviation = exchangeRate.TargetCurrencyAbbreviation,
                ExchangeRate = exchangeRate.ExchangeRate,
                ValidFrom = exchangeRate.ValidFrom,
                ValidTo = exchangeRate.ValidTo,
                IsCustom = true
            });
        }
        
        [GraphQLNonNullType]
        [Authorize(Policy = AuthOptions.AUTH_POLICY)]
        public Task<CurrencyExchangeRate> UpdateExchangeRate(
            [Service] IContextProvider contextProvider,
            [GraphQLNonNullType] UpdateExchangeRateInput exchangeRate)
        {
            return contextProvider.GetRepository<ICurrencyExchangeRateRepository>().Update(new CurrencyExchangeRate
            {
                Id = exchangeRate.Id,
                SourceCurrencyAbbreviation = exchangeRate.SourceCurrencyAbbreviation,
                TargetCurrencyAbbreviation = exchangeRate.TargetCurrencyAbbreviation,
                ExchangeRate = exchangeRate.ExchangeRate ?? default,
                ValidFrom = exchangeRate.ValidFrom ?? default,
                ValidTo = exchangeRate.ValidTo ?? default
            });
        }
        
        [GraphQLNonNullType]
        [Authorize(Policy = AuthOptions.AUTH_POLICY)]
        public Task<CurrencyExchangeRate> DeleteExchangeRate([Service] IContextProvider contextProvider, [GraphQLNonNullType] int id)
        {
            return contextProvider.GetRepository<ICurrencyExchangeRateRepository>().Delete(id);
        }
    }
}
