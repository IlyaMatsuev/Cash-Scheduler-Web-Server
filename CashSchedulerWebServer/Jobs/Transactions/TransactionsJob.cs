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
            var now = DateTime.UtcNow;
            Console.WriteLine($"Running the {context.JobDetail.Description}");
            var transactions = CashSchedulerContext.Transactions
                .Where(t => t.Date.Date == now.Date)
                .Include(t => t.Category)
                .Include(t => t.Category.Type)
                .Include(t => t.Wallet)
                .ToList();

            var walletsToUpdateBalance = transactions.GroupBy(t => t.Wallet).Select(t =>
            {
                var wallet = t.Key;
                double initBalance = wallet.Balance;
                wallet.Balance += t.Sum(
                    transaction => transaction.Category.Type.Name == TransactionType.Options.Income.ToString() 
                        ? transaction.Amount 
                        : -transaction.Amount
                );
                if (wallet.Balance < 0)
                {
                    wallet.Balance = initBalance;
                    // TODO: send email notifying that we cannot create transaction for future since the balance will become less than 0
                }
                return wallet;
            }).ToList();


            using var dmlTransaction = CashSchedulerContext.Database.BeginTransaction();
            try
            {
                CashSchedulerContext.Wallets.UpdateRange(walletsToUpdateBalance);
                CashSchedulerContext.SaveChanges();

                dmlTransaction.Commit();

                Console.WriteLine($"{walletsToUpdateBalance.Count} wallets were updated");
            }
            catch (Exception error)
            {
                dmlTransaction.Rollback();
                Console.WriteLine($"Error while running the {context.JobDetail.Description}: {error.Message}: \n{error.StackTrace}");
            }

            return Task.CompletedTask;
        }
    }
}
