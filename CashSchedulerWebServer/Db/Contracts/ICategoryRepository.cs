using CashSchedulerWebServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CashSchedulerWebServer.Db.Contracts
{
    interface ICategoryRepository : IRepository<Category>
    {
        public IEnumerable<Category> GetAll(string type);
        public IEnumerable<Category> GetStandardCategories();
        public IEnumerable<Category> GetCustomCategories();
        
    }
}
