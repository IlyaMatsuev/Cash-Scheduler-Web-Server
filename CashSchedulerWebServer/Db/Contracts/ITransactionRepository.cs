using CashSchedulerWebServer.Models;
using System.Collections.Generic;

namespace CashSchedulerWebServer.Db.Contracts
{
    public interface ITransactionRepository : IRepository<int, Transaction>
    {
        IEnumerable<Transaction> GetDashboardTransactions(int month, int year);
        IEnumerable<Transaction> GetTransactionsByMonth(int month, int year);
    }
}
