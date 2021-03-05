using CashSchedulerWebServer.Db;
using CashSchedulerWebServer.Exceptions;
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
        private CashSchedulerContext CashSchedulerContext { get; }

        public RecurringTransactionsJob(CashSchedulerContext cashSchedulerContext)
        {
            CashSchedulerContext = cashSchedulerContext;
        }


        public Task Execute(IJobExecutionContext context)
        {
            /*var now = DateTime.UtcNow;
            Console.WriteLine($"Running the {context.JobDetail.Description}");
            var recurringTransactions = CashSchedulerContext.RegularTransactions
                .Where(t => t.NextTransactionDate.Date == now.Date)
                .Include(t => t.Category)
                .Include(t => t.Category.Type)
                .Include(t => t.User)
                .ToList();

            var singleTransactionsToBeCreated = new List<Transaction>();
            var recurringTransactionsToBeUpdated = new List<RegularTransaction>();
            var usersToUpdateBalance = recurringTransactions.GroupBy(t => t.User).Select(t =>
            {
                var user = t.Key;
                singleTransactionsToBeCreated.AddRange(t.Select(rt => new Transaction
                {
                    Title = rt.Title,
                    User = rt.User,
                    Category = rt.Category,
                    Amount = rt.Amount
                }));
                recurringTransactionsToBeUpdated.AddRange(t.Select(rt =>
                {
                    rt.Date = DateTime.Today;
                    rt.NextTransactionDate = GetNextDateByInterval(rt);
                    return rt;
                }));
                user.Balance += t.Sum(rt => rt.Category.Type.Name == TransactionType.Options.Income.ToString() ? rt.Amount : -rt.Amount);
                return user;
            }).ToList();

            using var dmlTransaction = CashSchedulerContext.Database.BeginTransaction();
            try
            {
                CashSchedulerContext.Transactions.AddRange(singleTransactionsToBeCreated);
                CashSchedulerContext.SaveChanges();
                CashSchedulerContext.RegularTransactions.UpdateRange(recurringTransactionsToBeUpdated);
                CashSchedulerContext.SaveChanges();
                CashSchedulerContext.Users.UpdateRange(usersToUpdateBalance);
                CashSchedulerContext.SaveChanges();

                dmlTransaction.Commit();

                Console.WriteLine($"{singleTransactionsToBeCreated.Count} single transactions were created");
                Console.WriteLine($"{usersToUpdateBalance.Count} users were updated");
            }
            catch (Exception error)
            {
                dmlTransaction.Rollback();
                Console.WriteLine($"Error while running the {context.JobDetail.Description}: {error.Message}: \n{error.StackTrace}");
            }*/

            return Task.CompletedTask;
        }

        private DateTime GetNextDateByInterval(RegularTransaction transaction)
        {
            var intervals = new Dictionary<string, Func<DateTime, DateTime>>
            {
                {RegularTransaction.IntervalOptions.Day.ToString().ToLower(), (date) => date.AddDays(1) },
                {RegularTransaction.IntervalOptions.Week.ToString().ToLower(), (date) => date.AddDays(7) },
                {RegularTransaction.IntervalOptions.Month.ToString().ToLower(), (date) => date.AddMonths(1) },
                {RegularTransaction.IntervalOptions.Year.ToString().ToLower(), (date) => date.AddYears(1) }
            };

            if (!intervals.ContainsKey(transaction.Interval))
            {
                throw new CashSchedulerException($"There is no such value for interval: {transaction.Interval}");                
            }

            return intervals[transaction.Interval](transaction.NextTransactionDate);

        }
    }
}
