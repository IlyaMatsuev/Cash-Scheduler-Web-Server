using System.Threading.Tasks;
using CashSchedulerWebServer.Auth;
using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Models;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Types;

namespace CashSchedulerWebServer.Mutations.RecurringTransactions
{
    [ExtendObjectType(Name = "Mutation")]
    public class RecurringTransactionMutations
    {
        [GraphQLNonNullType]
        [Authorize(Policy = AuthOptions.AUTH_POLICY)]
        public Task<RegularTransaction> CreateRegularTransaction(
            [Service] IContextProvider contextProvider,
            [GraphQLNonNullType] NewRecurringTransactionInput transaction)
        {
            return contextProvider.GetRepository<IRegularTransactionRepository>().Create(new RegularTransaction
            {
                Title = transaction.Title,
                CategoryId = transaction.CategoryId,
                Amount = transaction.Amount,
                Date = transaction.NextTransactionDate,
                Interval = transaction.Interval
            });
        }
        
        [GraphQLNonNullType]
        [Authorize(Policy = AuthOptions.AUTH_POLICY)]
        public Task<RegularTransaction> UpdateRegularTransaction(
            [Service] IContextProvider contextProvider,
            [GraphQLNonNullType] UpdateRecurringTransactionInput transaction)
        {
            return contextProvider.GetRepository<IRegularTransactionRepository>().Update(new RegularTransaction
            {
                Id = transaction.Id,
                Title = transaction.Title,
                Amount = transaction.Amount ?? default
            });
        }
        
        [GraphQLNonNullType]
        [Authorize(Policy = AuthOptions.AUTH_POLICY)]
        public Task<RegularTransaction> DeleteRegularTransaction([Service] IContextProvider contextProvider, [GraphQLNonNullType] int id)
        {
            return contextProvider.GetRepository<IRegularTransactionRepository>().Delete(id);
        }
    }
}
