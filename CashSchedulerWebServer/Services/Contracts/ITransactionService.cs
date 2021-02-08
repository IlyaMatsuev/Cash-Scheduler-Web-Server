using System.Collections.Generic;
using CashSchedulerWebServer.Models;

namespace CashSchedulerWebServer.Services.Contracts
{
    public interface ITransactionService : IService<int, Transaction>
    {
        IEnumerable<Transaction> GetDashboardTransactions(int month, int year);
        IEnumerable<Transaction> GetTransactionsByMonth(int month, int year);
        
    }
}
