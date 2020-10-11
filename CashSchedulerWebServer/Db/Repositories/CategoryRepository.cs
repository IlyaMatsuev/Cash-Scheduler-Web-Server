using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Models;
using CashSchedulerWebServer.Utils;
using GraphQL;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CashSchedulerWebServer.Db.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private CashSchedulerContext Context { get; set; }
        private IContextProvider ContextProvider { get; set; }
        private ClaimsPrincipal User { get; set; }
        private int? UserId => Convert.ToInt32(User?.Claims.FirstOrDefault(claim => claim.Type == "Id")?.Value ?? "-1");

        public CategoryRepository(CashSchedulerContext context, IHttpContextAccessor httpAccessor, IContextProvider contextProvider)
        {
            Context = context;
            User = httpAccessor.HttpContext.User;
            ContextProvider = contextProvider;
        }


        public IEnumerable<Category> GetAll()
        {
            return Context.Categories.Where(c => (c.CreatedBy != null && c.CreatedBy.Id == UserId && c.IsCustom) || !c.IsCustom)
                .Include(c => c.Type)
                .Include(c => c.CreatedBy);
        }

        public IEnumerable<Category> GetAll(string type)
        {
            if (string.IsNullOrEmpty(type))
            {
                return GetAll();
            }
            return Context.Categories.Where(c => c.Type.Name == type && ((c.CreatedBy != null && c.CreatedBy.Id == UserId && c.IsCustom) || !c.IsCustom))
                .Include(c => c.Type)
                .Include(c => c.CreatedBy);
        }

        public IEnumerable<Category> GetStandardCategories()
        {
            return Context.Categories.Where(c => !c.IsCustom)
                .Include(c => c.Type)
                .Include(c => c.CreatedBy);
        }

        public IEnumerable<Category> GetCustomCategories()
        {
            return Context.Categories.Where(c => c.IsCustom && c.CreatedBy != null && c.CreatedBy.Id == UserId)
                .Include(c => c.Type)
                .Include(c => c.CreatedBy);
        }

        public Category GetById(int id)
        {
            return Context.Categories.Where(c => c.Id == id && c.CreatedBy.Id == UserId)
                .Include(c => c.Type)
                .Include(c => c.CreatedBy)
                .FirstOrDefault();
        }

        public async Task<Category> Create(Category category)
        {
            ModelValidator.ValidateModelAttributes(category);
            category.Type = ContextProvider.GetRepository<ITransactionTypeRepository>().GetByName(category.TransactionTypeName);
            if (category.Type == null)
            {
                throw new ExecutionError("There is no such transaction type");
            }
            category.CreatedBy = ContextProvider.GetRepository<IUserRepository>().GetById((int)UserId);
            category.IsCustom = true;

            Context.Categories.Add(category);
            await Context.SaveChangesAsync();

            return GetById(category.Id);
        }

        public async Task<Category> Update(Category category)
        {
            var targetCategory = GetById(category.Id);
            if (targetCategory == null)
            {
                throw new ExecutionError("There is no such category");
            }

            if (!string.IsNullOrEmpty(category.Name))
            {
                targetCategory.Name = category.Name;
            }
            if (!string.IsNullOrEmpty(category.IconUrl))
            {
                targetCategory.IconUrl = category.IconUrl;
            }

            ModelValidator.ValidateModelAttributes(targetCategory);

            Context.Categories.Update(targetCategory);
            await Context.SaveChangesAsync();

            return targetCategory;
        }

        public async Task<Category> Delete(int categoryId)
        {
            var targetCategory = GetById(categoryId);
            if (targetCategory == null)
            {
                throw new ExecutionError("There is no such category");
            }

            Context.Categories.Remove(targetCategory);
            await Context.SaveChangesAsync();

            return targetCategory;
        }
    }
}
