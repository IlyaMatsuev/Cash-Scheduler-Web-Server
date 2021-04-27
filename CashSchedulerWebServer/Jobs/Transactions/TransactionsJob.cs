using CashSchedulerWebServer.Db;
using CashSchedulerWebServer.Models;
using Microsoft.EntityFrameworkCore;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CashSchedulerWebServer.Services.Contracts;
using CashSchedulerWebServer.WebServices.Contracts;
using CashSchedulerWebServer.WebServices.Salesforce.SObjects;
using Microsoft.Extensions.DependencyInjection;

namespace CashSchedulerWebServer.Jobs.Transactions
{
    public class TransactionsJob : IJob
    {
        private CashSchedulerContext CashSchedulerContext { get; }
        private IUserNotificationService NotificationService { get; }
        private ISalesforceApiWebService SalesforceService { get; }

        public TransactionsJob(CashSchedulerContext cashSchedulerContext, IServiceProvider serviceProvider)
        {
            CashSchedulerContext = cashSchedulerContext;
            NotificationService = serviceProvider.GetService<IUserNotificationService>();
            SalesforceService = serviceProvider.GetService<ISalesforceApiWebService>();
        }


        public Task Execute(IJobExecutionContext context)
        {
            Console.WriteLine($"Running the {context.JobDetail.Description}");

            using var dmlTransaction = CashSchedulerContext.Database.BeginTransaction();
            try
            {
                var updatedWallets = GetUpdatedWallets(GetTodayTransactions());

                CashSchedulerContext.Wallets.UpdateRange(updatedWallets);
                CashSchedulerContext.SaveChanges();

                dmlTransaction.Commit();
                SalesforceService.UpsertSObjects(updatedWallets.Select(w => new SfWallet(w, w.Id)).ToList<SfObject>());

                Console.WriteLine($"{updatedWallets.Count} wallets were updated");
            }
            catch (Exception error)
            {
                dmlTransaction.Rollback();
                Console.WriteLine($"Error while running the {context.JobDetail.Description}: {error.Message}: \n{error.StackTrace}");
            }

            return Task.CompletedTask;
        }


        private List<Transaction> GetTodayTransactions()
        {
            var now = DateTime.UtcNow;
            return CashSchedulerContext.Transactions
                .Where(t => t.Date.Date == now.Date)
                .Include(t => t.Category)
                .Include(t => t.Category.Type)
                .Include(t => t.Wallet)
                .ToList();
        }

        private List<Wallet> GetUpdatedWallets(List<Transaction> transactions)
        {
            return transactions.GroupBy(t => t.Wallet).Select(UpdateWallet).ToList();
        }

        private Wallet UpdateWallet(IGrouping<Wallet, Transaction> transactionsByWallet)
        {
            var wallet = transactionsByWallet.Key;
            double initBalance = wallet.Balance;

            wallet.Balance += transactionsByWallet.Sum(CalculateAmount);
            if (wallet.Balance < 0)
            {
                NotifyNotEnoughMoney(transactionsByWallet);
                wallet.Balance = initBalance;
            }
            return wallet;
        }

        private string GetTransactionName(Transaction transaction)
        {
            return string.IsNullOrEmpty(transaction.Title)
                ? transaction.Category.Name
                : transaction.Title;
        }

        private double CalculateAmount(Transaction transaction)
        {
            return transaction.Category.Type.Name == TransactionType.Options.Income.ToString()
                ? transaction.Amount
                : -transaction.Amount;
        }

        private void NotifyNotEnoughMoney(IGrouping<Wallet, Transaction> transactionsByWallet)
        {
            var transactionNames = string.Join(',', transactionsByWallet.Select(GetTransactionName));

            NotificationService.Create(new UserNotification
            {
                Title = "Future transaction charge",
                Content = $"We're sorry but you don't have enough money on the wallet \"{transactionsByWallet.Key.Name}\" for the transactions: {transactionNames}",
                User = transactionsByWallet.Key.User
            });
        }
    }
}
