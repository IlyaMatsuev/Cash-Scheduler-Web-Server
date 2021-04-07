using System.Globalization;

namespace CashSchedulerWebServer.Queries.Transactions
{
    public class TransactionDelta
    {
        public string Month { get; }

        public double Delta { get; }

        public TransactionDelta(int month, double delta)
        {
            Month = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month);
            Delta = delta;
        }
    }
}
