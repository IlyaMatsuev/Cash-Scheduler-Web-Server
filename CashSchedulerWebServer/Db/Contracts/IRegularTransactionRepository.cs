using CashSchedulerWebServer.Models;
using System.Collections.Generic;

namespace CashSchedulerWebServer.Db.Contracts
{
    public interface IRegularTransactionRepository : IRepository<int, RegularTransaction>
    {
        IEnumerable<RegularTransaction> GetByCategoryId(int categoryId);
        IEnumerable<RegularTransaction> GetDashboardRegularTransactions(int month, int year);
        IEnumerable<RegularTransaction> GetRegularTransactionsByMonth(int month, int year);
    }
}
