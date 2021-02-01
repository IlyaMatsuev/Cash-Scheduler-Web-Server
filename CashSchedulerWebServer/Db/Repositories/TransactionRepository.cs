using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Exceptions;
using CashSchedulerWebServer.Models;
using CashSchedulerWebServer.Utils;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CashSchedulerWebServer.Auth.Contracts;

namespace CashSchedulerWebServer.Db.Repositories
{
    public class TransactionRepository : ITransactionRepository
    {
        private CashSchedulerContext Context { get; }
        private IContextProvider ContextProvider { get; }
        private int UserId { get; }

        public TransactionRepository(CashSchedulerContext context, IUserContext userContext, IContextProvider contextProvider)
        {
            Context = context;
            ContextProvider = contextProvider;
            UserId = userContext.GetUserId();
        }


        public IEnumerable<Transaction> GetAll()
        {
            return Context.Transactions.Where(t => t.User.Id == UserId)
                .Include(t => t.User)
                .Include(t => t.Category)
                .Include(t => t.Category.Type);
        }

        public IEnumerable<Transaction> GetByCategoryId(int categoryId)
        {
            return Context.Transactions.Where(t => t.Category.Id == categoryId);
        }

        public IEnumerable<Transaction> GetDashboardTransactions(int month, int year)
        {
            DateTime datePoint = new DateTime(year, month, 1);
            return Context.Transactions.Where(t => t.Date >= datePoint.AddMonths(-1) && t.Date <= datePoint.AddMonths(2) && t.User.Id == UserId)
                .Include(t => t.User)
                .Include(t => t.Category)
                .Include(t => t.Category.Type);
        }

        public IEnumerable<Transaction> GetTransactionsByMonth(int month, int year)
        {
            return Context.Transactions.Where(t => t.Date.Month == month && t.Date.Year == year && t.User.Id == UserId)
                .Include(t => t.User)
                .Include(t => t.Category)
                .Include(t => t.Category.Type);
        }

        public Transaction GetById(int id)
        {
            return Context.Transactions.Where(t => t.Id == id && t.User.Id == UserId)
                .Include(t => t.User)
                .Include(t => t.Category)
                .Include(t => t.Category.Type)
                .FirstOrDefault();
        }

        public async Task<Transaction> Create(Transaction transaction)
        {
            ModelValidator.ValidateModelAttributes(transaction);
            transaction.User = ContextProvider.GetRepository<IUserRepository>().GetById(UserId);
            transaction.Category = ContextProvider.GetRepository<ICategoryRepository>().GetById(transaction.CategoryId);
            if (transaction.Category == null)
            {
                throw new CashSchedulerException("There is no such category", new[] { "categoryId" });
            }
            Context.Transactions.Add(transaction);
            await Context.SaveChangesAsync();
            await ContextProvider.GetRepository<IUserRepository>().UpdateBalance(transaction, null, isCreate: true);

            return GetById(transaction.Id);
        }

        public async Task<Transaction> Update(Transaction transaction)
        {
            var targetTransaction = GetById(transaction.Id);
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

            ModelValidator.ValidateModelAttributes(targetTransaction);

            Context.Transactions.Update(targetTransaction);
            await Context.SaveChangesAsync();
            await ContextProvider.GetRepository<IUserRepository>().UpdateBalance(targetTransaction, oldTransaction, isUpdate: true);

            return targetTransaction;
        }

        public async Task<Transaction> Delete(int transactionId)
        {
            var targetTransaction = GetById(transactionId);
            if (targetTransaction == null)
            {
                throw new CashSchedulerException("There is no such transaction");
            }

            Context.Transactions.Remove(targetTransaction);
            await Context.SaveChangesAsync();
            await ContextProvider.GetRepository<IUserRepository>().UpdateBalance(targetTransaction, targetTransaction, isDelete: true);

            return targetTransaction;
        }
    }
}
