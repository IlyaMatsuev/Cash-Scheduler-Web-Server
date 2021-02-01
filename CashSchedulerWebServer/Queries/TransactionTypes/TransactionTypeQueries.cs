using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Models;
using HotChocolate;
using HotChocolate.Types;
using System.Collections.Generic;

#nullable enable

namespace CashSchedulerWebServer.Queries.TransactionTypes
{
    [ExtendObjectType(Name = "Query")]
    public class TransactionTypeQueries
    {
        public IEnumerable<TransactionType> TransactionTypes([Service] IContextProvider contextProvider)
        {
            return contextProvider.GetRepository<ITransactionTypeRepository>().GetAll();
        }
    }
}
