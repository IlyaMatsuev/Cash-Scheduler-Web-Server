using System.Collections.Generic;
using System.Threading.Tasks;
using CashSchedulerWebServer.Auth.Contracts;
using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Exceptions;
using CashSchedulerWebServer.Models;
using CashSchedulerWebServer.Utils;

namespace CashSchedulerWebServer.Services.Transactions
{
    public class RecurringTransactionService
    {
        private IContextProvider ContextProvider { get; }
        private int UserId { get; }

        public RecurringTransactionService(IContextProvider contextProvider, IUserContext userContext)
        {
            ContextProvider = contextProvider;
            UserId = userContext.GetUserId();
        }
        

        public IEnumerable<RegularTransaction> GetDashboardRegularTransactions(int month, int year)
        {
            return ContextProvider.GetRepository<IRegularTransactionRepository>().GetDashboardRegularTransactions(month, year);
        }

        public IEnumerable<RegularTransaction> GetRegularTransactionsByMonth(int month, int year)
        {
            return ContextProvider.GetRepository<IRegularTransactionRepository>().GetRegularTransactionsByMonth(month, year);
        }

        public Task<RegularTransaction> Create(RegularTransaction transaction)
        {
            transaction.User = ContextProvider.GetRepository<IUserRepository>().GetById(UserId);
            
            transaction.Category = ContextProvider.GetRepository<ICategoryRepository>().GetById(transaction.CategoryId);
            if (transaction.Category == null)
            {
                throw new CashSchedulerException("There is no such category", new[] { "categoryId" });
            }

            ModelValidator.ValidateModelAttributes(transaction);

            return ContextProvider.GetRepository<IRegularTransactionRepository>().Create(transaction);
        }

        public Task<RegularTransaction> Update(RegularTransaction transaction)
        {
            var transactionRepository = ContextProvider.GetRepository<IRegularTransactionRepository>();
            
            var targetTransaction = transactionRepository.GetById(transaction.Id);
            if (targetTransaction == null)
            {
                throw new CashSchedulerException("There is no such transaction");
            }

            targetTransaction.Title = transaction.Title;
            
            if (transaction.Amount != default)
            {
                targetTransaction.Amount = transaction.Amount;
            }

            ModelValidator.ValidateModelAttributes(targetTransaction);

            return transactionRepository.Update(transaction);
        }

        public Task<RegularTransaction> Delete(int id)
        {
            var transactionRepository = ContextProvider.GetRepository<IRegularTransactionRepository>();

            var targetTransaction = transactionRepository.GetById(id);
            if (targetTransaction == null)
            {
                throw new CashSchedulerException("There is no such transaction");
            }

            return transactionRepository.Delete(id);
        }
    }
}
