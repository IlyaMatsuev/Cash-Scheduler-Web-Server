using System.Threading.Tasks;
using CashSchedulerWebServer.Auth;
using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Models;
using CashSchedulerWebServer.Services.Contracts;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Types;

namespace CashSchedulerWebServer.Mutations.CurrencyExchangeRates
{
    [ExtendObjectType(Name = "Mutation")]
    public class CurrencyExchangeRateMutations
    {
        [GraphQLNonNullType]
        [Authorize(Policy = AuthOptions.AUTH_POLICY, Roles = new[] {AuthOptions.USER_ROLE})]
        public Task<CurrencyExchangeRate> CreateExchangeRate(
            [Service] IContextProvider contextProvider,
            [GraphQLNonNullType] NewExchangeRateInput exchangeRate)
        {
            return contextProvider.GetService<ICurrencyExchangeRateService>().Create(new CurrencyExchangeRate
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
        [Authorize(Policy = AuthOptions.AUTH_POLICY, Roles = new[] {AuthOptions.USER_ROLE})]
        public Task<CurrencyExchangeRate> UpdateExchangeRate(
            [Service] IContextProvider contextProvider,
            [GraphQLNonNullType] UpdateExchangeRateInput exchangeRate)
        {
            return contextProvider.GetService<ICurrencyExchangeRateService>().Update(new CurrencyExchangeRate
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
        [Authorize(Policy = AuthOptions.AUTH_POLICY, Roles = new[] {AuthOptions.USER_ROLE})]
        public Task<CurrencyExchangeRate> DeleteExchangeRate(
            [Service] IContextProvider contextProvider,
            [GraphQLNonNullType] int id)
        {
            return contextProvider.GetService<ICurrencyExchangeRateService>().Delete(id);
        }
    }
}
