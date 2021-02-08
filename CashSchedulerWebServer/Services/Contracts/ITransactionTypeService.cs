using System.Collections.Generic;
using CashSchedulerWebServer.Models;

namespace CashSchedulerWebServer.Services.Contracts
{
    public interface ITransactionTypeService : IService<string, TransactionType>
    {
        IEnumerable<TransactionType> GetAll();
        TransactionType GetByKey(string key);
    }
}
