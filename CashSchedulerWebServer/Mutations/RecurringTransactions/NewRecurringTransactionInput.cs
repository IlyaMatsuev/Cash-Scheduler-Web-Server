using System;
using HotChocolate;

namespace CashSchedulerWebServer.Mutations.RecurringTransactions
{
    public class NewRecurringTransactionInput
    {
        public string Title { get; set; }
        
        public int CategoryId { get; set; }
        
        public double Amount { get; set; }
        
        public DateTime NextTransactionDate { get; set; }
        
        [GraphQLNonNullType]
        public string Interval { get; set; }
    }
}
