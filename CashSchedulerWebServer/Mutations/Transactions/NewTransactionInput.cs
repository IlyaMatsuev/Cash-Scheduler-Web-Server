using System;
using HotChocolate;
using HotChocolate.Types;

namespace CashSchedulerWebServer.Mutations.Transactions
{
    public class NewTransactionInput
    {
        public string Title { get; set; }
        
        public int CategoryId { get; set; }
        
        public double Amount { get; set; }
        
        [GraphQLType(typeof(DateType))]
        public DateTime? Date { get; set; }
    }
}
