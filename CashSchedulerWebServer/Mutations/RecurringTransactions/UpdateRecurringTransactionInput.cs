namespace CashSchedulerWebServer.Mutations.RecurringTransactions
{
    public class UpdateRecurringTransactionInput
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public double? Amount { get; set; }
    }
}
