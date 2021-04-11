using CashSchedulerWebServer.Db.Contracts;
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
        private int UserId { get; }

        public TransactionRepository(CashSchedulerContext context, IUserContext userContext)
        {
            Context = context;
            UserId = userContext.GetUserId();
        }


        public Transaction GetByKey(int id)
        {
            return Context.Transactions.Where(t => t.Id == id && t.User.Id == UserId)
                .Include(t => t.User)
                .Include(t => t.Category)
                .Include(t => t.Category.Type)
                .Include(t => t.Wallet)
                .Include(t => t.Wallet.Currency)
                .FirstOrDefault();
        }
        
        public IEnumerable<Transaction> GetAll()
        {
            return Context.Transactions.Where(t => t.User.Id == UserId)
                .Include(t => t.User)
                .Include(t => t.Category)
                .Include(t => t.Category.Type)
                .Include(t => t.Wallet)
                .Include(t => t.Wallet.Currency);
        }

        public IEnumerable<Transaction> GetDashboardTransactions(int month, int year)
        {
            DateTime datePoint = new DateTime(year, month, 1);
            return Context.Transactions
                .Where(t => t.Date >= datePoint.AddMonths(-1) && t.Date <= datePoint.AddMonths(2) && t.User.Id == UserId)
                .Include(t => t.User)
                .Include(t => t.Category)
                .Include(t => t.Category.Type)
                .Include(t => t.Wallet)
                .Include(t => t.Wallet.Currency);
        }

        public IEnumerable<Transaction> GetTransactionsByMonth(int month, int year)
        {
            return Context.Transactions
                .Where(t => t.Date.Month == month && t.Date.Year == year && t.User.Id == UserId)
                .Include(t => t.User)
                .Include(t => t.Category)
                .Include(t => t.Category.Type)
                .Include(t => t.Wallet)
                .Include(t => t.Wallet.Currency);
        }

        public IEnumerable<Transaction> GetTransactionsByYear(int year)
        {
            return Context.Transactions
                .Where(t => t.User.Id == UserId && t.Date.Year == year)
                .Include(t => t.Category)
                .Include(t => t.Category.Type);
        }

        public async Task<Transaction> Create(Transaction transaction)
        {
            ModelValidator.ValidateModelAttributes(transaction);
            
            await Context.Transactions.AddAsync(transaction);
            await Context.SaveChangesAsync();

            return GetByKey(transaction.Id);
        }

        public async Task<Transaction> Update(Transaction transaction)
        {
            ModelValidator.ValidateModelAttributes(transaction);

            Context.Transactions.Update(transaction);
            await Context.SaveChangesAsync();

            return GetByKey(transaction.Id);
        }

        public async Task<Transaction> Delete(int id)
        {
            var transaction = GetByKey(id);

            Context.Transactions.Remove(transaction);
            await Context.SaveChangesAsync();

            return transaction;
        }

        public async Task<IEnumerable<Transaction>> DeleteByCategoryId(int categoryId)
        {
            var transactions = Context.Transactions.Where(t => t.Category.Id == categoryId)
                .Include(t => t.User)
                .Include(t => t.Category)
                .Include(t => t.Category.Type)
                .Include(t => t.Wallet)
                .Include(t => t.Wallet.Currency)
                .ToList();

            Context.Transactions.RemoveRange(transactions);
            await Context.SaveChangesAsync();
            return transactions;
        }

        public IEnumerable<Transaction> DeleteByUserId(int userId)
        {
            var transactions = Context.Transactions.Where(c => c.User.Id == userId);

            Context.Transactions.RemoveRange(transactions);
            Context.SaveChanges();

            return transactions;
        }
    }
}
