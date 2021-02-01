using System;

namespace CashSchedulerWebServer.Mutations.Transactions
{
    public class NewTransactionInput
    {
        public string Title { get; set; }
        
        public int CategoryId { get; set; }
        
        public double Amount { get; set; }
        
        public DateTime? Date { get; set; }
    }
}
