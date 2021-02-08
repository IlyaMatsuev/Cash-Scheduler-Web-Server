using System.Collections.Generic;
using CashSchedulerWebServer.Models;

namespace CashSchedulerWebServer.Services.Contracts
{
    public interface ICategoryService : IService<int, Category>
    {
        IEnumerable<Category> GetAll(string transactionType = null);
        IEnumerable<Category> GetStandardCategories(string transactionType = null);
        IEnumerable<Category> GetCustomCategories(string transactionType = null);
    }
}
