using System.Collections.Generic;
using CashSchedulerWebServer.Auth;
using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Models;
using CashSchedulerWebServer.Services.Contracts;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Types;

#nullable enable

namespace CashSchedulerWebServer.Queries.RecurringTransactions
{
    [ExtendObjectType(Name = "Query")]
    public class RecurringTransactionQueries
    {
        [Authorize(Policy = AuthOptions.AUTH_POLICY)]
        public IEnumerable<RegularTransaction>? DashboardRecurringTransactions(
            [Service] IContextProvider contextProvider,
            int month,
            int year)
        {
            return contextProvider.GetService<IRecurringTransactionService>()
                .GetDashboardRegularTransactions(month, year);
        }

        [Authorize(Policy = AuthOptions.AUTH_POLICY)]
        public IEnumerable<RegularTransaction>? RecurringTransactionsByMonth(
            [Service] IContextProvider contextProvider,
            int month,
            int year)
        {
            return contextProvider.GetService<IRecurringTransactionService>()
                .GetRegularTransactionsByMonth(month, year);
        }
    }
}
