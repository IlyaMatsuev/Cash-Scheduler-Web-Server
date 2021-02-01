using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CashSchedulerWebServer.Auth.Contracts;
using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Exceptions;
using CashSchedulerWebServer.Models;
using CashSchedulerWebServer.Utils;
using Microsoft.EntityFrameworkCore;

namespace CashSchedulerWebServer.Db.Repositories
{
    public class RegularTransactionRepository : IRegularTransactionRepository
    {
        private CashSchedulerContext Context { get; }
        private IContextProvider ContextProvider { get; }
        private int UserId { get; }

        public RegularTransactionRepository(CashSchedulerContext context, IUserContext userContext, IContextProvider contextProvider)
        {
            Context = context;
            ContextProvider = contextProvider;
            UserId = userContext.GetUserId();
        }


        public IEnumerable<RegularTransaction> GetAll()
        {
            return Context.RegularTransactions.Where(t => t.User.Id == UserId)
                .Include(t => t.User)
                .Include(t => t.Category)
                .Include(t => t.Category.Type);
        }

        public IEnumerable<RegularTransaction> GetDashboardRegularTransactions(int month, int year)
        {
            DateTime datePoint = new DateTime(year, month, 1);
            return Context.RegularTransactions
                .Where(t => t.NextTransactionDate >= datePoint.AddMonths(-1) && t.NextTransactionDate <= datePoint.AddMonths(2) && t.User.Id == UserId)
                .Include(t => t.User)
                .Include(t => t.Category)
                .Include(t => t.Category.Type);
        }

        public IEnumerable<RegularTransaction> GetRegularTransactionsByMonth(int month, int year)
        {
            return Context.RegularTransactions
                .Where(t => t.NextTransactionDate.Month == month && t.NextTransactionDate.Year == year && t.User.Id == UserId)
                .Include(t => t.User)
                .Include(t => t.Category)
                .Include(t => t.Category.Type);
        }

        public RegularTransaction GetById(int id)
        {
            return Context.RegularTransactions.Where(t => t.Id == id && t.User.Id == UserId)
                .Include(t => t.User)
                .Include(t => t.Category)
                .Include(t => t.Category.Type)
                .FirstOrDefault();
        }

        public IEnumerable<RegularTransaction> GetByCategoryId(int categoryId)
        {
            return Context.RegularTransactions.Where(t => t.Category.Id == categoryId);
        }

        public async Task<RegularTransaction> Create(RegularTransaction transaction)
        {
            ModelValidator.ValidateModelAttributes(transaction);
            transaction.User = ContextProvider.GetRepository<IUserRepository>().GetById(UserId);
            transaction.Category = ContextProvider.GetRepository<ICategoryRepository>().GetById(transaction.CategoryId);
            if (transaction.Category == null)
            {
                throw new CashSchedulerException("There is no such category", new[] { "categoryId" });
            }
            Context.RegularTransactions.Add(transaction);
            await Context.SaveChangesAsync();

            return GetById(transaction.Id);
        }

        public async Task<RegularTransaction> Update(RegularTransaction transaction)
        {
            var targetTransaction = GetById(transaction.Id);
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

            Context.RegularTransactions.Update(targetTransaction);
            await Context.SaveChangesAsync();

            return targetTransaction;
        }

        public async Task<RegularTransaction> Delete(int transactionId)
        {
            var targetTransaction = GetById(transactionId);
            if (targetTransaction == null)
            {
                throw new CashSchedulerException("There is no such transaction");
            }

            Context.RegularTransactions.Remove(targetTransaction);
            await Context.SaveChangesAsync();

            return targetTransaction;
        }
    }
}
