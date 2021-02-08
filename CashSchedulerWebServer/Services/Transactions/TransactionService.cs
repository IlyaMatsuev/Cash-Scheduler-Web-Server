using System.Collections.Generic;
using System.Threading.Tasks;
using CashSchedulerWebServer.Auth.Contracts;
using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Exceptions;
using CashSchedulerWebServer.Models;
using CashSchedulerWebServer.Services.Contracts;
using CashSchedulerWebServer.Utils;

namespace CashSchedulerWebServer.Services.Transactions
{
    public class TransactionService : ITransactionService
    {
        private IContextProvider ContextProvider { get; }
        private int UserId { get; }

        public TransactionService(IContextProvider contextProvider, IUserContext userContext)
        {
            ContextProvider = contextProvider;
            UserId = userContext.GetUserId();
        }
        

        public IEnumerable<Transaction> GetDashboardTransactions(int month, int year)
        {
            return ContextProvider.GetRepository<ITransactionRepository>().GetDashboardTransactions(month, year);
        }

        public IEnumerable<Transaction> GetTransactionsByMonth(int month, int year)
        {
            return ContextProvider.GetRepository<ITransactionRepository>().GetTransactionsByMonth(month, year);
        }

        public async Task<Transaction> Create(Transaction transaction)
        {
            var transactionRepository = ContextProvider.GetRepository<ITransactionRepository>();
            
            transaction.User = ContextProvider.GetRepository<IUserRepository>().GetById(UserId);
            
            transaction.Category = ContextProvider.GetRepository<ICategoryRepository>().GetById(transaction.CategoryId);
            if (transaction.Category == null)
            {
                throw new CashSchedulerException("There is no such category", new[] { "categoryId" });
            }

            transaction = await transactionRepository.Create(transaction);
            
            //await ContextProvider.GetRepository<IUserRepository>().UpdateBalance(transaction, null, true);

            return transaction;
        }

        public async Task<Transaction> Update(Transaction transaction)
        {
            var transactionRepository = ContextProvider.GetRepository<ITransactionRepository>();

            var targetTransaction = transactionRepository.GetById(transaction.Id);
            if (targetTransaction == null)
            {
                throw new CashSchedulerException("There is no such transaction");
            }

            var oldTransaction = new Transaction
            {
                Title = targetTransaction.Title,
                Amount = targetTransaction.Amount,
                Date = targetTransaction.Date,
                Category = targetTransaction.Category
            };

            targetTransaction.Title = transaction.Title;

            if (transaction.Amount != default)
            {
                targetTransaction.Amount = transaction.Amount;
            }
            
            if (transaction.Date != default)
            {
                targetTransaction.Date = transaction.Date;
            }

            targetTransaction = await transactionRepository.Update(targetTransaction);
            
            //await ContextProvider.GetRepository<IUserRepository>().UpdateBalance(targetTransaction, oldTransaction, isUpdate: true);

            return targetTransaction;
        }

        public async Task<Transaction> Delete(int transactionId)
        {
            var transactionRepository = ContextProvider.GetRepository<ITransactionRepository>();

            var targetTransaction = transactionRepository.GetById(transactionId);
            if (targetTransaction == null)
            {
                throw new CashSchedulerException("There is no such transaction");
            }

            targetTransaction = await transactionRepository.Delete(transactionId);
            
            //await ContextProvider.GetRepository<IUserRepository>().UpdateBalance(targetTransaction, targetTransaction, isDelete: true);

            return targetTransaction;
        }
    }
}
