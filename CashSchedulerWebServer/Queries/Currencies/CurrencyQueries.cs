using System.Collections.Generic;
using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Models;
using CashSchedulerWebServer.Services.Contracts;
using HotChocolate;
using HotChocolate.Types;

#nullable enable

namespace CashSchedulerWebServer.Queries.Currencies
{
    [ExtendObjectType(Name = "Query")]
    public class CurrencyQueries
    {
        public IEnumerable<Currency> Currencies([Service] IContextProvider contextProvider)
        {
            return contextProvider.GetService<ICurrencyService>().GetAll();
        }
    }
}
