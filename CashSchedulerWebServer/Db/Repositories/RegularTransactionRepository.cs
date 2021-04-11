using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CashSchedulerWebServer.Auth.Contracts;
using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Models;
using CashSchedulerWebServer.Utils;
using Microsoft.EntityFrameworkCore;

namespace CashSchedulerWebServer.Db.Repositories
{
    public class RegularTransactionRepository : IRegularTransactionRepository
    {
        private CashSchedulerContext Context { get; }
        private int UserId { get; }

        public RegularTransactionRepository(CashSchedulerContext context, IUserContext userContext)
        {
            Context = context;
            UserId = userContext.GetUserId();
        }


        public IEnumerable<RegularTransaction> GetAll()
        {
            return Context.RegularTransactions.Where(t => t.User.Id == UserId)
                .Include(t => t.User)
                .Include(t => t.Category)
                .Include(t => t.Category.Type)
                .Include(t => t.Wallet)
                .Include(t => t.Wallet.Currency);
        }

        public IEnumerable<RegularTransaction> GetDashboardRegularTransactions(int month, int year)
        {
            DateTime datePoint = new DateTime(year, month, 1);
            return Context.RegularTransactions
                .Where(t => t.NextTransactionDate >= datePoint.AddMonths(-1) && t.NextTransactionDate <= datePoint.AddMonths(2) && t.User.Id == UserId)
                .Include(t => t.User)
                .Include(t => t.Category)
                .Include(t => t.Category.Type)
                .Include(t => t.Wallet)
                .Include(t => t.Wallet.Currency);
        }

        public IEnumerable<RegularTransaction> GetRegularTransactionsByMonth(int month, int year)
        {
            return Context.RegularTransactions
                .Where(t => t.NextTransactionDate.Month == month && t.NextTransactionDate.Year == year && t.User.Id == UserId)
                .Include(t => t.User)
                .Include(t => t.Category)
                .Include(t => t.Category.Type)
                .Include(t => t.Wallet)
                .Include(t => t.Wallet.Currency);
        }

        public IEnumerable<RegularTransaction> GetRegularTransactionsByYear(int year)
        {
            return Context.RegularTransactions
                .Where(t => t.User.Id == UserId)
                .Where(t => t.NextTransactionDate > DateTime.Today && t.NextTransactionDate.Year == year)
                .Include(t => t.Category)
                .Include(t => t.Category.Type);
        }

        public RegularTransaction GetByKey(int id)
        {
            return Context.RegularTransactions.Where(t => t.Id == id && t.User.Id == UserId)
                .Include(t => t.User)
                .Include(t => t.Category)
                .Include(t => t.Category.Type)
                .Include(t => t.Wallet)
                .Include(t => t.Wallet.Currency)
                .FirstOrDefault();
        }

        public async Task<RegularTransaction> Create(RegularTransaction transaction)
        {
            ModelValidator.ValidateModelAttributes(transaction);
            
            await Context.RegularTransactions.AddAsync(transaction);
            await Context.SaveChangesAsync();

            return GetByKey(transaction.Id);
        }

        public async Task<RegularTransaction> Update(RegularTransaction transaction)
        {
            ModelValidator.ValidateModelAttributes(transaction);

            Context.RegularTransactions.Update(transaction);
            await Context.SaveChangesAsync();

            return transaction;
        }

        public async Task<RegularTransaction> Delete(int id)
        {
            var transaction = GetByKey(id);

            Context.RegularTransactions.Remove(transaction);
            await Context.SaveChangesAsync();

            return transaction;
        }

        public async Task<IEnumerable<RegularTransaction>> DeleteByCategoryId(int categoryId)
        {
            var transactions = Context.RegularTransactions.Where(t => t.Category.Id == categoryId)
                .Include(t => t.User)
                .Include(t => t.Category)
                .Include(t => t.Category.Type)
                .Include(t => t.Wallet)
                .Include(t => t.Wallet.Currency)
                .ToList();

            Context.RegularTransactions.RemoveRange(transactions);
            await Context.SaveChangesAsync();
            return transactions;
        }

        public  IEnumerable<RegularTransaction> DeleteByUserId(int userId)
        {
            var transactions = Context.RegularTransactions.Where(c => c.User.Id == userId);

            Context.RegularTransactions.RemoveRange(transactions);
            Context.SaveChanges();

            return transactions;
        }
    }
}
