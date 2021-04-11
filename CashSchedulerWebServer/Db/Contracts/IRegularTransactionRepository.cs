using CashSchedulerWebServer.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CashSchedulerWebServer.Db.Contracts
{
    public interface IRegularTransactionRepository : IRepository<int, RegularTransaction>
    {
        IEnumerable<RegularTransaction> GetDashboardRegularTransactions(int month, int year);

        IEnumerable<RegularTransaction> GetRegularTransactionsByMonth(int month, int year);

        IEnumerable<RegularTransaction> GetRegularTransactionsByYear(int year);

        Task<IEnumerable<RegularTransaction>> DeleteByCategoryId(int categoryId);

        IEnumerable<RegularTransaction> DeleteByUserId(int userId);
    }
}
