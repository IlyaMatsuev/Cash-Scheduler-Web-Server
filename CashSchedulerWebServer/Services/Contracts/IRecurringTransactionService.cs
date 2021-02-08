using System.Collections.Generic;
using CashSchedulerWebServer.Models;

namespace CashSchedulerWebServer.Services.Contracts
{
    public interface IRecurringTransactionService : IService<int, RegularTransaction>
    {
        IEnumerable<RegularTransaction> GetDashboardRegularTransactions(int month, int year);
        IEnumerable<RegularTransaction> GetRegularTransactionsByMonth(int month, int year);
    }
}
