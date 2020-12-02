using CashSchedulerWebServer.Models;
using System.Collections.Generic;

namespace CashSchedulerWebServer.Db.Contracts
{
    interface IRegularTransactionRepository : IRepository<RegularTransaction>
    {
        IEnumerable<RegularTransaction> GetAll(int size);
        IEnumerable<RegularTransaction> GetByCategoryId(int categoryId);
    }
}
