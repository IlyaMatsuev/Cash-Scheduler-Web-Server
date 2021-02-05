using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Exceptions;
using CashSchedulerWebServer.Models;
using CashSchedulerWebServer.Utils;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CashSchedulerWebServer.Auth.Contracts;

namespace CashSchedulerWebServer.Db.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private CashSchedulerContext Context { get; }
        private IContextProvider ContextProvider { get; }
        private int UserId { get; }

        public CategoryRepository(CashSchedulerContext context, IUserContext userContext, IContextProvider contextProvider)
        {
            Context = context;
            ContextProvider = contextProvider;
            UserId = userContext.GetUserId();
        }


        public IEnumerable<Category> GetAll()
        {
            return Context.Categories
                .Where(c => (c.User != null && c.User.Id == UserId && c.IsCustom) || !c.IsCustom)
                .Include(c => c.Type)
                .Include(c => c.User);
        }

        public IEnumerable<Category> GetAll(string transactionType)
        {
            if (string.IsNullOrEmpty(transactionType))
            {
                return GetAll();
            }
            
            return Context.Categories
                .Where(c => c.Type.Name == transactionType 
                            && ((c.User != null && c.User.Id == UserId && c.IsCustom) || !c.IsCustom))
                .Include(c => c.Type)
                .Include(c => c.User);
        }

        public IEnumerable<Category> GetStandardCategories(string transactionType)
        {
            if (string.IsNullOrEmpty(transactionType))
            {
                return Context.Categories.Where(c => !c.IsCustom)
                    .Include(c => c.Type)
                    .Include(c => c.User);
            }
            
            return Context.Categories.Where(c => !c.IsCustom && c.Type.Name == transactionType)
                .Include(c => c.Type)
                .Include(c => c.User);
        }

        public IEnumerable<Category> GetCustomCategories(string transactionType)
        {
            if (string.IsNullOrEmpty(transactionType))
            {
                return Context.Categories
                    .Where(c => c.IsCustom 
                                && c.User != null 
                                && c.User.Id == UserId)
                    .Include(c => c.Type)
                    .Include(c => c.User);
            }
            
            return Context.Categories
                .Where(c => c.IsCustom 
                            && c.User != null
                            && c.User.Id == UserId 
                            && c.Type.Name == transactionType)
                .Include(c => c.Type)
                .Include(c => c.User);
        }

        public Category GetById(int id)
        {
            return Context.Categories.
                Where(c => c.Id == id && ((c.User.Id == UserId && c.IsCustom) || !c.IsCustom))
                .Include(c => c.Type)
                .Include(c => c.User)
                .FirstOrDefault();
        }

        public async Task<Category> Create(Category category)
        {
            ModelValidator.ValidateModelAttributes(category);
            category.Type = ContextProvider.GetRepository<ITransactionTypeRepository>().GetById(category.TypeName);
            if (category.Type == null)
            {
                throw new CashSchedulerException("There is no such transaction type", new[] { "transactionTypeName" });
            }
            category.User = ContextProvider.GetRepository<IUserRepository>().GetById(UserId);

            Context.Categories.Add(category);
            await Context.SaveChangesAsync();

            return category;
        }

        public async Task<Category> Update(Category category)
        {
            var targetCategory = GetById(category.Id);
            if (targetCategory == null)
            {
                throw new CashSchedulerException("There is no such category");
            }

            if (!string.IsNullOrEmpty(category.Name))
            {
                targetCategory.Name = category.Name;
            }
            if (!string.IsNullOrEmpty(category.IconUrl))
            {
                targetCategory.IconUrl = category.IconUrl;
            }

            targetCategory.TypeName = targetCategory.Type.Name;

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
                throw new CashSchedulerException("There is no such category");
            }
            
            if (!targetCategory.IsCustom)
            {
                throw new CashSchedulerException("You cannot delete one of the standard categories");
            }

            var relatedTransactions = ContextProvider.GetRepository<ITransactionRepository>().GetByCategoryId(categoryId);
            var relatedRegularTransactions = ContextProvider.GetRepository<IRegularTransactionRepository>().GetByCategoryId(categoryId);

            Context.Transactions.RemoveRange(relatedTransactions);
            Context.RegularTransactions.RemoveRange(relatedRegularTransactions);
            Context.Categories.Remove(targetCategory);
            await Context.SaveChangesAsync();

            return targetCategory;
        }
    }
}
