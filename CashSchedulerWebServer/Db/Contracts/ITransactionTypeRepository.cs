using CashSchedulerWebServer.Models;

namespace CashSchedulerWebServer.Db.Contracts
{
    interface ITransactionTypeRepository : IRepository<TransactionType>
    {
        TransactionType GetByName(string name);
    }
}
