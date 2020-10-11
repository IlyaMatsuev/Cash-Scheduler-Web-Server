﻿using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Models;
using CashSchedulerWebServer.Utils;
using GraphQL;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CashSchedulerWebServer.Db.Repositories
{
    public class TransactionRepository : ITransactionRepository
    {
        private CashSchedulerContext Context { get; set; }
        private IContextProvider ContextProvider { get; set; }
        private ClaimsPrincipal User { get; set; }
        private int? UserId => Convert.ToInt32(User?.Claims.FirstOrDefault(claim => claim.Type == "Id")?.Value ?? "-1");

        public TransactionRepository(CashSchedulerContext context, IHttpContextAccessor httpAccessor, IContextProvider contextProvider)
        {
            Context = context;
            ContextProvider = contextProvider;
            User = httpAccessor.HttpContext.User;
        }


        public IEnumerable<Transaction> GetAll(int size)
        {
            IEnumerable<Transaction> transactions = Context.Transactions.Where(t => t.CreatedBy.Id == UserId)
                .Include(t => t.CreatedBy)
                .Include(t => t.TransactionCategory)
                .Include(t => t.TransactionCategory.Type);

            if (size == 0)
            {
                return transactions;
            }
            return transactions.Take(size);
        }

        public IEnumerable<Transaction> GetTransactionsForLastDays(int days)
        {
            DateTime daysAgo = DateTime.Today.AddDays(-days);
            return Context.Transactions.Where(t => t.Date >= daysAgo && t.Date <= DateTime.Today && t.CreatedBy.Id == UserId)
                .Include(t => t.CreatedBy)
                .Include(t => t.TransactionCategory)
                .Include(t => t.TransactionCategory.Type);
        }

        public IEnumerable<Transaction> GetTransactionsByMonth(int month, int year)
        {
            return Context.Transactions.Where(t => t.Date.Month == month && t.Date.Year == year && t.CreatedBy.Id == UserId)
                .Include(t => t.CreatedBy)
                .Include(t => t.TransactionCategory)
                .Include(t => t.TransactionCategory.Type);
        }

        public IEnumerable<Transaction> GetAll()
        {
            return GetAll(0);
        }

        public Transaction GetById(int id)
        {
            return Context.Transactions.Where(t => t.Id == id && t.CreatedBy.Id == UserId)
                .Include(t => t.CreatedBy)
                .Include(t => t.TransactionCategory)
                .Include(t => t.TransactionCategory.Type)
                .FirstOrDefault();
        }

        public async Task<Transaction> Create(Transaction transaction)
        {
            ModelValidator.ValidateModelAttributes(transaction);
            transaction.CreatedBy = ContextProvider.GetRepository<IUserRepository>().GetById((int)UserId);
            transaction.Date = transaction.Date == default ? DateTime.UtcNow : transaction.Date;
            transaction.TransactionCategory = ContextProvider.GetRepository<ICategoryRepository>().GetById(transaction.CategoryId);
            if (transaction.TransactionCategory == null)
            {
                throw new ExecutionError("There is no such category");
            }
            Context.Transactions.Add(transaction);
            await Context.SaveChangesAsync();

            return GetById(transaction.Id);
        }

        public async Task<Transaction> Update(Transaction transaction)
        {
            var targetTransaction = GetById(transaction.Id);
            if (targetTransaction == null)
            {
                throw new ExecutionError("There is no such transaction");
            }

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

            return targetTransaction;
        }

        public async Task<Transaction> Delete(int transactionId)
        {
            var targetTransaction = GetById(transactionId);
            if (targetTransaction == null)
            {
                throw new ExecutionError("There is no such transaction");
            }

            Context.Transactions.Remove(targetTransaction);
            await Context.SaveChangesAsync();

            return targetTransaction;
        }
    }
}
