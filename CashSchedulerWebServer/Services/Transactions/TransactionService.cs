﻿using System.Collections.Generic;
using System.Threading.Tasks;
using CashSchedulerWebServer.Auth.Contracts;
using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Exceptions;
using CashSchedulerWebServer.Models;
using CashSchedulerWebServer.Services.Contracts;

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

            transaction.User = ContextProvider.GetRepository<IUserRepository>().GetByKey(UserId);

            transaction.Category = ContextProvider.GetRepository<ICategoryRepository>().GetByKey(transaction.CategoryId);
            if (transaction.Category == null)
            {
                throw new CashSchedulerException("There is no such category", new[] { "categoryId" });
            }

            transaction.Wallet = transaction.WalletId == default
                ? ContextProvider.GetRepository<IWalletRepository>().GetDefault()
                : ContextProvider.GetRepository<IWalletRepository>().GetByKey(transaction.WalletId);

            if (transaction.Wallet == null)
            {
                throw new CashSchedulerException("There is no such wallet", new[] { "walletId" });
            }

            if (transaction.Category.Type.Name == TransactionType.Options.Expense.ToString()
                && transaction.Wallet.Balance < transaction.Amount)
            {
                throw new CashSchedulerException("Amount cannot be greater than the balance", new[] { "amount" });
            }

            transaction = await transactionRepository.Create(transaction);

            await ContextProvider.GetService<IWalletService>().UpdateBalance(transaction, null, true);

            return transaction;
        }

        public async Task<Transaction> Update(Transaction transaction)
        {
            var transactionRepository = ContextProvider.GetRepository<ITransactionRepository>();

            var targetTransaction = transactionRepository.GetByKey(transaction.Id);
            if (targetTransaction == null)
            {
                throw new CashSchedulerException("There is no such transaction");
            }

            var oldTransaction = new Transaction
            {
                Title = targetTransaction.Title,
                Amount = targetTransaction.Amount,
                Date = targetTransaction.Date,
                Category = targetTransaction.Category,
                Wallet = targetTransaction.Wallet
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

            if (oldTransaction.Category.Type.Name == TransactionType.Options.Expense.ToString()
                && transaction.Amount - oldTransaction.Amount > 0
                && oldTransaction.Wallet.Balance < transaction.Amount - oldTransaction.Amount)
            {
                throw new CashSchedulerException("Amount cannot be greater than the balance", new[] { "amount" });
            }

            targetTransaction = await transactionRepository.Update(targetTransaction);

            await ContextProvider.GetService<IWalletService>().UpdateBalance(targetTransaction, oldTransaction, isUpdate: true);

            return targetTransaction;
        }

        public async Task<Transaction> Delete(int id)
        {
            var transactionRepository = ContextProvider.GetRepository<ITransactionRepository>();

            var transaction = transactionRepository.GetByKey(id);
            if (transaction == null)
            {
                throw new CashSchedulerException("There is no such transaction");
            }

            transaction = await transactionRepository.Delete(id);
            
            await ContextProvider.GetService<IWalletService>().UpdateBalance(transaction, transaction, isDelete: true);

            return transaction;
        }
    }
}
