using CashSchedulerWebServer.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CashSchedulerWebServer.Db.Contracts
{
    public interface ITransactionRepository : IRepository<int, Transaction>
    {
        IEnumerable<Transaction> GetDashboardTransactions(int month, int year);

        IEnumerable<Transaction> GetTransactionsByMonth(int month, int year);

        IEnumerable<Transaction> GetTransactionsByYear(int year);

        Task<IEnumerable<Transaction>> DeleteByCategoryId(int categoryId);

        IEnumerable<Transaction> DeleteByUserId(int userId);
    }
}
