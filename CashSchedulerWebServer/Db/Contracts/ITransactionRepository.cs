using CashSchedulerWebServer.Models;
using System.Collections.Generic;

namespace CashSchedulerWebServer.Db.Contracts
{
    interface ITransactionRepository : IRepository<Transaction>
    {
        IEnumerable<Transaction> GetAll(int size);
        IEnumerable<Transaction> GetByCategoryId(int categoryId);
        IEnumerable<Transaction> GetTransactionsForLastDays(int days);
        IEnumerable<Transaction> GetTransactionsByMonth(int month, int year);
    }
}
