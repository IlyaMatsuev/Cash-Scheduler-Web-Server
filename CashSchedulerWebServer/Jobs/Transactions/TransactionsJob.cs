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
    public class TransactionsJob : IJob
    {
        private readonly CashSchedulerContext cashSchedulerContext;

        public TransactionsJob(CashSchedulerContext cashSchedulerContext)
        {
            this.cashSchedulerContext = cashSchedulerContext;
        }


        public Task Execute(IJobExecutionContext context)
        {
            var now = DateTime.UtcNow;
            Console.WriteLine($"Running the {context.JobDetail.Description}");
            var transactions = cashSchedulerContext.Transactions.Where(t => t.Date.Date == now.Date)
                .Include(t => t.TransactionCategory)
                .Include(t => t.TransactionCategory.Type)
                .Include(t => t.CreatedBy)
                .ToList();

            List<User> usersToUpdateBalance = transactions.GroupBy(t => t.CreatedBy).Select(t =>
            {
                User user = t.Key;
                user.Balance += t.Sum(transaction => transaction.TransactionCategory.Type.Name == TransactionType.Options.Income.ToString() ? transaction.Amount : -transaction.Amount);
                return user;
            }).ToList();


            using (var dmlTransaction = cashSchedulerContext.Database.BeginTransaction())
            {
                try
                {
                    cashSchedulerContext.Users.UpdateRange(usersToUpdateBalance);
                    cashSchedulerContext.SaveChanges();

                    dmlTransaction.Commit();

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
    }
}
