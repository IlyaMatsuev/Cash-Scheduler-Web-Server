using System.Collections.Generic;
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
        [Authorize(Policy = AuthOptions.AUTH_POLICY)]
        public IEnumerable<CurrencyExchangeRate>? ExchangeRates([Service] IContextProvider contextProvider)
        {
            return contextProvider.GetService<ICurrencyExchangeRateService>().GetAll();
        }
    }
}
