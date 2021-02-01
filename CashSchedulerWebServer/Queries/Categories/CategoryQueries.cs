using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Models;
using HotChocolate;
using HotChocolate.Types;
using System.Collections.Generic;
using CashSchedulerWebServer.Auth;
using HotChocolate.AspNetCore.Authorization;

#nullable enable

namespace CashSchedulerWebServer.Queries.Categories
{
    [ExtendObjectType(Name = "Query")]
    public class CategoryQueries
    {
        [Authorize(Policy = AuthOptions.AUTH_POLICY)]
        public IEnumerable<Category>? AllCategories([Service] IContextProvider contextProvider, string? transactionType)
        {
            return contextProvider.GetRepository<ICategoryRepository>().GetAll(transactionType);
        }

        public IEnumerable<Category> StandardCategories([Service] IContextProvider contextProvider, string? transactionType)
        {
            return contextProvider.GetRepository<ICategoryRepository>().GetStandardCategories(transactionType);
        }
        
        [Authorize(Policy = AuthOptions.AUTH_POLICY)]
        public IEnumerable<Category>? CustomCategories([Service] IContextProvider contextProvider, string? transactionType)
        {
            return contextProvider.GetRepository<ICategoryRepository>().GetCustomCategories(transactionType);
        }
    }
}
