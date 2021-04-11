using CashSchedulerWebServer.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CashSchedulerWebServer.Db.Contracts
{
    public interface ICategoryRepository : IRepository<int, Category>
    {
        IEnumerable<Category> GetAll(string transactionType);

        IEnumerable<Category> GetStandardCategories(string transactionType = null);

        IEnumerable<Category> GetCustomCategories(string transactionType = null);

        IEnumerable<Category> DeleteByUserId(int userId);
    }
}
