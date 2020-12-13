using CashSchedulerWebServer.Db;
using CashSchedulerWebServer.Models;
using Microsoft.EntityFrameworkCore;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CashSchedulerWebServer.Jobs.Transactions
{
    public class RecurringTransactionsJob : IJob
    {
        private readonly CashSchedulerContext cashSchedulerContext;

        public RecurringTransactionsJob(CashSchedulerContext cashSchedulerContext)
        {
            this.cashSchedulerContext = cashSchedulerContext;
        }


        public Task Execute(IJobExecutionContext context)
        {
            var now = DateTime.UtcNow;
            Console.WriteLine($"Running the {context.JobDetail.Description}");
            var recurringTransactions = cashSchedulerContext.RegularTransactions.Where(t => t.NextTransactionDate.Date == now.Date)
                .Include(t => t.TransactionCategory)
                .Include(t => t.TransactionCategory.Type)
                .Include(t => t.CreatedBy)
                .ToList();

            var singleTransactionsToBeCreated = new List<Transaction>();
            var recurringTransactionsToBeUpdated = new List<RegularTransaction>();
            List<User> usersToUpdateBalance = recurringTransactions.GroupBy(t => t.CreatedBy).Select(t =>
            {
                User user = t.Key;
                singleTransactionsToBeCreated.AddRange(t.Select(rt => new Transaction
                {
                    Title = rt.Title,
                    CreatedBy = rt.CreatedBy,
                    TransactionCategory = rt.TransactionCategory,
                    Amount = rt.Amount
                }));
                recurringTransactionsToBeUpdated.AddRange(t.Select(rt =>
                {
                    rt.NextTransactionDate = GetNextDateByInterval(rt);
                    return rt;
                }));
                user.Balance += t.Sum(rt => rt.TransactionCategory.Type.Name == "Income" ? rt.Amount : -rt.Amount);
                return user;
            }).ToList();

            using (var dmlTransaction = cashSchedulerContext.Database.BeginTransaction())
            {
                try
                {
                    cashSchedulerContext.Transactions.AddRange(singleTransactionsToBeCreated);
                    cashSchedulerContext.SaveChanges();
                    cashSchedulerContext.RegularTransactions.UpdateRange(recurringTransactionsToBeUpdated);
                    cashSchedulerContext.SaveChanges();
                    cashSchedulerContext.Users.UpdateRange(usersToUpdateBalance);
                    cashSchedulerContext.SaveChanges();

                    dmlTransaction.Commit();

                    Console.WriteLine($"{singleTransactionsToBeCreated.Count()} single transactions were created");
                    Console.WriteLine($"{usersToUpdateBalance.Count()} users were updated");
                }
                catch (Exception error)
                {
                    dmlTransaction.Rollback();
                    Console.WriteLine($"Error while running the {context.JobDetail.Description}: {error.Message}: \n{error.StackTrace}");
                }
            }

            return Task.CompletedTask;
        }

        private DateTime GetNextDateByInterval(RegularTransaction transaction)
        {
            // TODO: we don't want to use names like that. Probably move to enum or constants
            return transaction.Interval switch
            {
                "day" => transaction.NextTransactionDate.AddDays(1),
                "week" => transaction.NextTransactionDate.AddDays(7),
                "month" => transaction.NextTransactionDate.AddMonths(1),
                "year" => transaction.NextTransactionDate.AddYears(1),
                _ => transaction.NextTransactionDate,
            };
        }
    }
}
