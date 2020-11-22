using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Exceptions;
using CashSchedulerWebServer.Models;
using CashSchedulerWebServer.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CashSchedulerWebServer.Db.Repositories
{
    public class RegularTransactionRepository : IRegularTransactionRepository
    {
        private CashSchedulerContext Context { get; set; }
        private IContextProvider ContextProvider { get; set; }
        private ClaimsPrincipal User { get; set; }
        private int? UserId => Convert.ToInt32(User?.Claims.FirstOrDefault(claim => claim.Type == "Id")?.Value ?? "-1");

        public RegularTransactionRepository(CashSchedulerContext context, IHttpContextAccessor httpAccessor, IContextProvider contextProvider)
        {
            Context = context;
            ContextProvider = contextProvider;
            User = httpAccessor.HttpContext.User;
        }


        public IEnumerable<RegularTransaction> GetAll(int size)
        {
            IEnumerable<RegularTransaction> transactions = Context.RegularTransactions.Where(t => t.CreatedBy.Id == UserId)
                .Include(t => t.CreatedBy)
                .Include(t => t.TransactionCategory)
                .Include(t => t.TransactionCategory.Type);

            if (size == 0)
            {
                return transactions;
            }
            return transactions.Take(size);
        }

        public IEnumerable<RegularTransaction> GetAll()
        {
            return GetAll(0);
        }

        public RegularTransaction GetById(int id)
        {
            return Context.RegularTransactions.Where(t => t.Id == id && t.CreatedBy.Id == UserId)
                .Include(t => t.CreatedBy)
                .Include(t => t.TransactionCategory)
                .Include(t => t.TransactionCategory.Type)
                .FirstOrDefault();
        }

        public async Task<RegularTransaction> Create(RegularTransaction transaction)
        {
            ModelValidator.ValidateModelAttributes(transaction);
            transaction.CreatedBy = ContextProvider.GetRepository<IUserRepository>().GetById((int)UserId);
            transaction.Date = transaction.Date == default ? DateTime.UtcNow : transaction.Date;
            transaction.TransactionCategory = ContextProvider.GetRepository<ICategoryRepository>().GetById(transaction.CategoryId);
            if (transaction.TransactionCategory == null)
            {
                throw new CashSchedulerException("There is no such category", new string[] { "categoryId" });
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
