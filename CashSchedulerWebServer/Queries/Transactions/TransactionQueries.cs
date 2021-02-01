using System.Collections.Generic;
using CashSchedulerWebServer.Auth;
using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Models;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Types;

#nullable enable

namespace CashSchedulerWebServer.Queries.Transactions
{
    [ExtendObjectType(Name = "Query")]
    public class TransactionQueries
    {
        [Authorize(Policy = AuthOptions.AUTH_POLICY)]
        public IEnumerable<Transaction>? DashboardTransactions([Service] IContextProvider contextProvider, int month, int year)
        {
            return contextProvider.GetRepository<ITransactionRepository>().GetDashboardTransactions(month, year);
        }
        
        [Authorize(Policy = AuthOptions.AUTH_POLICY)]
        public IEnumerable<Transaction>? TransactionsByMonth([Service] IContextProvider contextProvider, int month, int year)
        {
            return contextProvider.GetRepository<ITransactionRepository>().GetTransactionsByMonth(month, year);
        }
    }
}
