﻿using System.Threading.Tasks;
using CashSchedulerWebServer.Auth;
using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Models;
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
        public Task<Category> CreateCategory([Service] IContextProvider contextProvider, [GraphQLNonNullType] NewCategoryInput category)
        {
            return contextProvider.GetRepository<ICategoryRepository>().Create(new Category
            {
                Name = category.Name,
                TypeName = category.TransactionTypeName,
                IconUrl = category.IconUrl
            });
        }
        
        [GraphQLNonNullType]
        [Authorize(Policy = AuthOptions.AUTH_POLICY)]
        public Task<Category> UpdateCategory([Service] IContextProvider contextProvider, [GraphQLNonNullType] UpdateCategoryInput category)
        {
            return contextProvider.GetRepository<ICategoryRepository>().Update(new Category
            {
                Id = category.Id,
                Name = category.Name,
                IconUrl = category.IconUrl
            });
        }
        
        [GraphQLNonNullType]
        [Authorize(Policy = AuthOptions.AUTH_POLICY)]
        public Task<Category> DeleteCategory([Service] IContextProvider contextProvider, [GraphQLNonNullType] int id)
        {
            return contextProvider.GetRepository<ICategoryRepository>().Delete(id);
        }
    }
}