using CashSchedulerWebServer.Db;
using CashSchedulerWebServer.Models;
using Microsoft.EntityFrameworkCore;
using Quartz;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CashSchedulerWebServer.Jobs.Transactions
{
    public class TransactionsJob : IJob
    {
        private CashSchedulerContext CashSchedulerContext { get; }

        public TransactionsJob(CashSchedulerContext cashSchedulerContext)
        {
            CashSchedulerContext = cashSchedulerContext;
        }


        public Task Execute(IJobExecutionContext context)
        {
            /*var now = DateTime.UtcNow;
            Console.WriteLine($"Running the {context.JobDetail.Description}");
            var transactions = CashSchedulerContext.Transactions
                .Where(t => t.Date.Date == now.Date)
                .Include(t => t.Category)
                .Include(t => t.Category.Type)
                .Include(t => t.User)
                .ToList();

            var usersToUpdateBalance = transactions.GroupBy(t => t.User).Select(t =>
            {
                var user = t.Key;
                user.Balance += t.Sum(
                    transaction => transaction.Category.Type.Name == TransactionType.Options.Income.ToString() 
                        ? transaction.Amount 
                        : -transaction.Amount
                );
                return user;
            }).ToList();


            using var dmlTransaction = CashSchedulerContext.Database.BeginTransaction();
            try
            {
                CashSchedulerContext.Users.UpdateRange(usersToUpdateBalance);
                CashSchedulerContext.SaveChanges();

                dmlTransaction.Commit();

                Console.WriteLine($"{usersToUpdateBalance.Count} users were updated");
            }
            catch (Exception error)
            {
                dmlTransaction.Rollback();
                Console.WriteLine($"Error while running the {context.JobDetail.Description}: {error.Message}: \n{error.StackTrace}");
            }*/

            return Task.CompletedTask;
        }
    }
}
