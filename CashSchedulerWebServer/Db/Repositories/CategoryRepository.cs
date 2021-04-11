using CashSchedulerWebServer.Db.Contracts;
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
        private int UserId { get; }

        public CategoryRepository(CashSchedulerContext context, IUserContext userContext)
        {
            Context = context;
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
            return Context.Categories
                .Where(c => c.Type.Name == transactionType 
                            && ((c.User != null && c.User.Id == UserId && c.IsCustom) || !c.IsCustom))
                .Include(c => c.Type)
                .Include(c => c.User);
        }

        public IEnumerable<Category> GetStandardCategories(string transactionType = null)
        {
            if (string.IsNullOrEmpty(transactionType))
            {
                return Context.Categories.Where(c => !c.IsCustom)
                    .Include(c => c.Type)
                    .Include(c => c.User);
            }

            return Context.Categories
                .Where(c => !c.IsCustom && c.Type.Name == transactionType)
                .Include(c => c.Type)
                .Include(c => c.User);
        }

        public IEnumerable<Category> GetCustomCategories(string transactionType = null)
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

        public Category GetByKey(int id)
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

            await Context.Categories.AddAsync(category);
            await Context.SaveChangesAsync();

            return category;
        }

        public async Task<Category> Update(Category category)
        {
            ModelValidator.ValidateModelAttributes(category);

            Context.Categories.Update(category);
            await Context.SaveChangesAsync();

            return category;
        }

        public async Task<Category> Delete(int id)
        {
            var category = GetByKey(id);

            Context.Categories.Remove(category);
            await Context.SaveChangesAsync();

            return category;
        }

        public IEnumerable<Category> DeleteByUserId(int userId)
        {
            var categories = Context.Categories.Where(c => c.IsCustom && c.User.Id == userId);

            Context.Categories.RemoveRange(categories);
            Context.SaveChanges();

            return categories;
        }
    }
}
