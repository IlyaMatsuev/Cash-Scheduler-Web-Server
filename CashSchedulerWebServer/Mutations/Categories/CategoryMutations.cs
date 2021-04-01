using System.Threading.Tasks;
using CashSchedulerWebServer.Auth;
using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Models;
using CashSchedulerWebServer.Services.Contracts;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Types;

namespace CashSchedulerWebServer.Mutations.Categories
{
    [ExtendObjectType(Name = "Mutation")]
    public class CategoryMutations
    {
        [GraphQLNonNullType]
        [Authorize(Policy = AuthOptions.AUTH_POLICY)]
        public Task<Category> CreateCategory(
            [Service] IContextProvider contextProvider,
            [GraphQLNonNullType] NewCategoryInput category)
        {
            return contextProvider.GetService<ICategoryService>().Create(new Category
            {
                Name = category.Name,
                TypeName = category.TransactionTypeName,
                IconUrl = category.IconUrl,
                IsCustom = true
            });
        }

        [GraphQLNonNullType]
        [Authorize(Policy = AuthOptions.AUTH_POLICY)]
        public Task<Category> UpdateCategory(
            [Service] IContextProvider contextProvider,
            [GraphQLNonNullType] UpdateCategoryInput category)
        {
            return contextProvider.GetService<ICategoryService>().Update(new Category
            {
                Id = category.Id,
                Name = category.Name,
                IconUrl = category.IconUrl
            });
        }

        [GraphQLNonNullType]
        [Authorize(Policy = AuthOptions.AUTH_POLICY, Roles = new[] {AuthOptions.USER_ROLE})]
        public Task<Category> DeleteCategory([Service] IContextProvider contextProvider, [GraphQLNonNullType] int id)
        {
            return contextProvider.GetService<ICategoryService>().Delete(id);
        }
    }
}
