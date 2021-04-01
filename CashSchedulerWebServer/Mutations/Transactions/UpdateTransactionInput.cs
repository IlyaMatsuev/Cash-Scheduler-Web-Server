using System;
using HotChocolate;
using HotChocolate.Types;

namespace CashSchedulerWebServer.Mutations.Transactions
{
    public class UpdateTransactionInput
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public double? Amount { get; set; }

        [GraphQLType(typeof(DateType))]
        public DateTime? Date { get; set; }
    }
}
