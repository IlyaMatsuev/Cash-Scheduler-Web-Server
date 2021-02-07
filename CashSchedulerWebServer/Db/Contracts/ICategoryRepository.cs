using CashSchedulerWebServer.Models;
using System.Collections.Generic;

namespace CashSchedulerWebServer.Db.Contracts
{
    public interface ICategoryRepository : IRepository<int, Category>
    {
        public IEnumerable<Category> GetAll(string transactionType);
        public IEnumerable<Category> GetStandardCategories(string transactionType = null);
        public IEnumerable<Category> GetCustomCategories(string transactionType = null);
    }
}
