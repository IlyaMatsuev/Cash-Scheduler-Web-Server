using CashSchedulerWebServer.Models;
using System.Collections.Generic;

namespace CashSchedulerWebServer.Db.Contracts
{
    interface ICategoryRepository : IRepository<Category>
    {
        public IEnumerable<Category> GetAll(string transactionType);
        public IEnumerable<Category> GetStandardCategories(string transactionType);
        public IEnumerable<Category> GetCustomCategories(string transactionType);
    }
}
