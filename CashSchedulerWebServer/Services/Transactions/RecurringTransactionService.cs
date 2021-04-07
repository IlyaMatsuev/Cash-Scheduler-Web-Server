using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CashSchedulerWebServer.Auth.Contracts;
using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Events;
using CashSchedulerWebServer.Events.Contracts;
using CashSchedulerWebServer.Exceptions;
using CashSchedulerWebServer.Models;
using CashSchedulerWebServer.Queries.Transactions;
using CashSchedulerWebServer.Services.Contracts;

namespace CashSchedulerWebServer.Services.Transactions
{
    public class RecurringTransactionService : IRecurringTransactionService
    {
        private IContextProvider ContextProvider { get; }
        private IEventManager EventManager { get; }
        private int UserId { get; }

        public RecurringTransactionService(
            IContextProvider contextProvider,
            IUserContext userContext,
            IEventManager eventManager)
        {
            ContextProvider = contextProvider;
            EventManager = eventManager;
            UserId = userContext.GetUserId();
        }


        public IEnumerable<RegularTransaction> GetDashboardRegularTransactions(int month, int year)
        {
            return ContextProvider.GetRepository<IRegularTransactionRepository>()
                .GetDashboardRegularTransactions(month, year);
        }

        public IEnumerable<RegularTransaction> GetRegularTransactionsByMonth(int month, int year)
        {
            return ContextProvider.GetRepository<IRegularTransactionRepository>()
                .GetRegularTransactionsByMonth(month, year);
        }

        public IEnumerable<TransactionDelta> GetRegularTransactionsDelta(int year)
        {
            var transactionsByYear = ContextProvider.GetRepository<IRegularTransactionRepository>()
                .GetRegularTransactionsByYear(year);

            var groupedByMonth = transactionsByYear.GroupBy(t => t.NextTransactionDate.Month);

            static bool IsIncome(RegularTransaction transaction) =>
                transaction.Category.Type.Name == TransactionType.Options.Income.ToString();

            return groupedByMonth.Select(transactions =>
            {
                return new TransactionDelta(
                    transactions.Key,
                    transactions.Sum(t => IsIncome(t) ? t.Amount : -t.Amount)
                );
            });
        }

        public async Task<RegularTransaction> Create(RegularTransaction transaction)
        {
            transaction.User = ContextProvider.GetRepository<IUserRepository>().GetByKey(UserId);

            transaction.Category = ContextProvider.GetRepository<ICategoryRepository>().GetByKey(transaction.CategoryId);
            if (transaction.Category == null)
            {
                throw new CashSchedulerException("There is no such category", new[] {"categoryId"});
            }

            transaction.Wallet = transaction.WalletId == default
                ? ContextProvider.GetRepository<IWalletRepository>().GetDefault()
                : ContextProvider.GetRepository<IWalletRepository>().GetByKey(transaction.WalletId);

            if (transaction.Wallet == null)
            {
                throw new CashSchedulerException("There is no such wallet", new[] {"walletId"});
            }

            var createdTransaction = await ContextProvider.GetRepository<IRegularTransactionRepository>().Create(transaction);

            await EventManager.FireEvent(EventAction.RecordUpserted, createdTransaction);

            return createdTransaction;
        }

        public async Task<RegularTransaction> Update(RegularTransaction transaction)
        {
            var transactionRepository = ContextProvider.GetRepository<IRegularTransactionRepository>();

            var targetTransaction = transactionRepository.GetByKey(transaction.Id);
            if (targetTransaction == null)
            {
                throw new CashSchedulerException("There is no such transaction");
            }

            targetTransaction.Title = transaction.Title;

            if (transaction.Amount != default)
            {
                targetTransaction.Amount = transaction.Amount;
            }

            var updatedTransaction = await transactionRepository.Update(targetTransaction);

            await EventManager.FireEvent(EventAction.RecordUpserted, updatedTransaction);

            return updatedTransaction;
        }

        public async Task<RegularTransaction> Delete(int id)
        {
            var transactionRepository = ContextProvider.GetRepository<IRegularTransactionRepository>();

            var targetTransaction = transactionRepository.GetByKey(id);
            if (targetTransaction == null)
            {
                throw new CashSchedulerException("There is no such transaction");
            }

            var deletedTransaction = await transactionRepository.Delete(id);

            await EventManager.FireEvent(EventAction.RecordDeleted, deletedTransaction);

            return deletedTransaction;
        }
    }
}
