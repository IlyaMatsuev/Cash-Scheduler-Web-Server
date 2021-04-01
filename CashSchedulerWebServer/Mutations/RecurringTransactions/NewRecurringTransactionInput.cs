using System;
using HotChocolate;
using HotChocolate.Types;

namespace CashSchedulerWebServer.Mutations.RecurringTransactions
{
    public class NewRecurringTransactionInput
    {
        public string Title { get; set; }

        public int CategoryId { get; set; }

        public int? WalletId { get; set; }

        public double Amount { get; set; }

        [GraphQLType(typeof(DateType))]
        public DateTime NextTransactionDate { get; set; }

        [GraphQLNonNullType]
        public string Interval { get; set; }
    }
}
