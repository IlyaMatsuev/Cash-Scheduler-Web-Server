using System.Collections.Generic;
using System.Threading.Tasks;
using CashSchedulerWebServer.Auth.Contracts;
using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Events;
using CashSchedulerWebServer.Events.Contracts;
using CashSchedulerWebServer.Exceptions;
using CashSchedulerWebServer.Models;
using CashSchedulerWebServer.Services.Contracts;

namespace CashSchedulerWebServer.Services.Categories
{
    public class CategoryService : ICategoryService
    {
        private IContextProvider ContextProvider { get; }
        private IEventManager EventManager { get; }
        private int UserId { get; }

        public CategoryService(IContextProvider contextProvider, IUserContext userContext, IEventManager eventManager)
        {
            ContextProvider = contextProvider;
            EventManager = eventManager;
            UserId = userContext.GetUserId();
        }


        public IEnumerable<Category> GetAll(string transactionType = null)
        {
            var categoryRepository = ContextProvider.GetRepository<ICategoryRepository>();

            return string.IsNullOrEmpty(transactionType)
                ? categoryRepository.GetAll()
                : categoryRepository.GetAll(transactionType);
        }

        public IEnumerable<Category> GetStandardCategories(string transactionType = null)
        {
            var categoryRepository = ContextProvider.GetRepository<ICategoryRepository>();

            return string.IsNullOrEmpty(transactionType)
                ? categoryRepository.GetStandardCategories()
                : categoryRepository.GetStandardCategories(transactionType);
        }

        public IEnumerable<Category> GetCustomCategories(string transactionType = null)
        {
            var categoryRepository = ContextProvider.GetRepository<ICategoryRepository>();

            return string.IsNullOrEmpty(transactionType)
                ? categoryRepository.GetCustomCategories()
                : categoryRepository.GetCustomCategories(transactionType);
        }

        public async Task<Category> Create(Category category)
        {
            category.Type = ContextProvider.GetRepository<ITransactionTypeRepository>().GetByKey(category.TypeName);

            if (category.Type == null)
            {
                throw new CashSchedulerException("There is no such transaction type", new[] {"transactionTypeName"});
            }

            category.User = ContextProvider.GetRepository<IUserRepository>().GetByKey(UserId);

            var createdCategory = await ContextProvider.GetRepository<ICategoryRepository>().Create(category);

            await EventManager.FireEvent(EventAction.RecordUpserted, createdCategory);

            return createdCategory;
        }

        public async Task<Category> Update(Category category)
        {
            var categoryRepository = ContextProvider.GetRepository<ICategoryRepository>();

            var targetCategory = categoryRepository.GetByKey(category.Id);
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

            var updatedCategory = await categoryRepository.Update(targetCategory);

            await EventManager.FireEvent(EventAction.RecordUpserted, updatedCategory);

            return updatedCategory;
        }

        public async Task<Category> Delete(int id)
        {
            var categoryRepository = ContextProvider.GetRepository<ICategoryRepository>();

            var category = categoryRepository.GetByKey(id);
            if (category == null)
            {
                throw new CashSchedulerException("There is no such category");
            }

            if (!category.IsCustom)
            {
                throw new CashSchedulerException("You cannot delete one of the standard categories");
            }

            var relatedTransactions = await ContextProvider.GetRepository<ITransactionRepository>().DeleteByCategoryId(id);
            await ContextProvider.GetRepository<IRegularTransactionRepository>().DeleteByCategoryId(id);

            await ContextProvider.GetService<IWalletService>().UpdateBalance(
                relatedTransactions,
                relatedTransactions,
                isDelete: true
            );

            var deletedCategory = await categoryRepository.Delete(id);

            await EventManager.FireEvent(EventAction.RecordDeleted, deletedCategory);

            return deletedCategory;
        }
    }
}
