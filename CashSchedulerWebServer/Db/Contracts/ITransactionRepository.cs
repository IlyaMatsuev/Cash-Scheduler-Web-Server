using CashSchedulerWebServer.Models;
using System.Collections.Generic;

namespace CashSchedulerWebServer.Db.Contracts
{
    interface ITransactionRepository : IRepository<Transaction>
    {
        IEnumerable<Transaction> GetByCategoryId(int categoryId);
        IEnumerable<Transaction> GetDashboardTransactions(int month, int year);
        IEnumerable<Transaction> GetTransactionsByMonth(int month, int year);
    }
}
