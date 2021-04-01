using System.Collections.Generic;
using System.Threading.Tasks;
using CashSchedulerWebServer.Auth;
using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Models;
using CashSchedulerWebServer.Services.Contracts;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Types;

#nullable enable

namespace CashSchedulerWebServer.Queries.CurrencyExchangeRates
{
    [ExtendObjectType(Name = "Query")]
    public class CurrencyExchangeRateQueries
    {
        [Authorize(Policy = AuthOptions.AUTH_POLICY, Roles = new[] {AuthOptions.USER_ROLE})]
        public Task<IEnumerable<CurrencyExchangeRate>>? ExchangeRates(
            [Service] IContextProvider contextProvider,
            [GraphQLNonNullType] string sourceCurrencyAbbreviation,
            [GraphQLNonNullType] string targetCurrencyAbbreviation
        )
        {
            return contextProvider.GetService<ICurrencyExchangeRateService>().GetBySourceAndTarget(
                sourceCurrencyAbbreviation,
                targetCurrencyAbbreviation
            );
        }
    }
}
