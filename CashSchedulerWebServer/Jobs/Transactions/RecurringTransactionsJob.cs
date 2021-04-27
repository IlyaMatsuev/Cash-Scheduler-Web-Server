using CashSchedulerWebServer.Db;
using CashSchedulerWebServer.Exceptions;
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
    public class RecurringTransactionsJob : IJob
    {
        private CashSchedulerContext CashSchedulerContext { get; }
        private IUserNotificationService NotificationService { get; }
        private ISalesforceApiWebService SalesforceService { get; }

        public RecurringTransactionsJob(CashSchedulerContext cashSchedulerContext, IServiceProvider serviceProvider)
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
                var newTransactions = new List<Transaction>();
                var updatedRecurringTransactions = new List<RegularTransaction>();
                var updatedWallets = GetUpdatedWallets(
                    GetTodayTransactions(),
                    newTransactions,
                    updatedRecurringTransactions
                );

                CashSchedulerContext.Transactions.AddRange(newTransactions);
                CashSchedulerContext.SaveChanges();

                CashSchedulerContext.RegularTransactions.UpdateRange(updatedRecurringTransactions);
                CashSchedulerContext.SaveChanges();

                CashSchedulerContext.Wallets.UpdateRange(updatedWallets);
                CashSchedulerContext.SaveChanges();

                dmlTransaction.Commit();
                
                SalesforceService.UpsertSObjects(newTransactions
                        .Select(t => new SfTransaction(t, t.Id)).ToList<SfObject>())
                    .GetAwaiter().GetResult();
                
                SalesforceService.UpsertSObjects(updatedRecurringTransactions
                        .Select(t => new SfRecurringTransaction(t, t.Id)).ToList<SfObject>())
                    .GetAwaiter().GetResult();
                
                SalesforceService.UpsertSObjects(updatedWallets.Select(w => new SfWallet(w, w.Id)).ToList<SfObject>())
                    .GetAwaiter().GetResult();

                Console.WriteLine($"{newTransactions.Count} single transactions were created");
                Console.WriteLine($"{updatedRecurringTransactions.Count} recurring transactions were updated");
                Console.WriteLine($"{updatedWallets.Count} wallets were updated");
            }
            catch (Exception error)
            {
                dmlTransaction.Rollback();
                Console.WriteLine($"Error while running the {context.JobDetail.Description}: {error.Message}: \n{error.StackTrace}");
            }

            return Task.CompletedTask;
        }


        private List<RegularTransaction> GetTodayTransactions()
        {
            var now = DateTime.UtcNow;
            return CashSchedulerContext.RegularTransactions
                .Where(t => t.NextTransactionDate.Date == now.Date)
                .Include(t => t.Category)
                .Include(t => t.Category.Type)
                .Include(t => t.User)
                .Include(t => t.Wallet)
                .ToList();
        }
        
        private List<Wallet> GetUpdatedWallets(
            List<RegularTransaction> transactions,
            List<Transaction> newTransactions,
            List<RegularTransaction> updatedRecurringTransactions)
        {
            return transactions.GroupBy(t => t.Wallet)
                .Select(t => UpdateWallet(t, newTransactions, updatedRecurringTransactions))
                .ToList();
        }

        private Wallet UpdateWallet(
            IGrouping<Wallet, RegularTransaction> transactionsByWallet,
            List<Transaction> newTransactions,
            List<RegularTransaction> updatedRecurringTransactions)
        {
            var wallet = transactionsByWallet.Key;
            double initBalance = wallet.Balance;

            wallet.Balance += transactionsByWallet.Sum(CalculateAmount);

            if (wallet.Balance < 0)
            {
                wallet.Balance = initBalance;
                NotifyNotEnoughMoney(transactionsByWallet);
            }
            else
            {
                newTransactions.AddRange(transactionsByWallet.Select(rt => new Transaction
                {
                    Title = rt.Title,
                    User = rt.User,
                    Wallet = rt.Wallet,
                    Category = rt.Category,
                    Amount = rt.Amount
                }));

                updatedRecurringTransactions.AddRange(transactionsByWallet.Select(rt =>
                {
                    rt.Date = DateTime.Today;
                    rt.NextTransactionDate = GetNextDateByInterval(rt);
                    return rt;
                }));
            }
            return wallet;
        }

        private string GetTransactionName(RegularTransaction transaction)
        {
            return string.IsNullOrEmpty(transaction.Title)
                ? transaction.Category.Name
                : transaction.Title;
        }

        private double CalculateAmount(RegularTransaction transaction)
        {
            return transaction.Category.Type.Name == TransactionType.Options.Income.ToString()
                ? transaction.Amount
                : -transaction.Amount;
        }

        private DateTime GetNextDateByInterval(RegularTransaction transaction)
        {
            var intervals = new Dictionary<string, Func<DateTime, DateTime>>
            {
                {RegularTransaction.IntervalOptions.Day.ToString().ToLower(), (date) => date.AddDays(1)},
                {RegularTransaction.IntervalOptions.Week.ToString().ToLower(), (date) => date.AddDays(7)},
                {RegularTransaction.IntervalOptions.Month.ToString().ToLower(), (date) => date.AddMonths(1)},
                {RegularTransaction.IntervalOptions.Year.ToString().ToLower(), (date) => date.AddYears(1)}
            };

            if (!intervals.ContainsKey(transaction.Interval))
            {
                throw new CashSchedulerException($"There is no such value for interval: {transaction.Interval}");
            }

            return intervals[transaction.Interval](transaction.NextTransactionDate);
        }

        private void NotifyNotEnoughMoney(IGrouping<Wallet, RegularTransaction> transactionsByWallet)
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
