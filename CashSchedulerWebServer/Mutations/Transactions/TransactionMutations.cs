using System;
using System.Threading.Tasks;
using CashSchedulerWebServer.Auth;
using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Models;
using CashSchedulerWebServer.Services.Contracts;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Types;

namespace CashSchedulerWebServer.Mutations.Transactions
{
    [ExtendObjectType(Name = "Mutation")]
    public class TransactionMutations
    {
        [GraphQLNonNullType]
        [Authorize(Policy = AuthOptions.AUTH_POLICY)]
        public Task<Transaction> CreateTransaction(
            [Service] IContextProvider contextProvider,
            [GraphQLNonNullType] NewTransactionInput transaction)
        {
            return contextProvider.GetService<ITransactionService>().Create(new Transaction
            {
                Title = transaction.Title,
                CategoryId = transaction.CategoryId,
                Amount = transaction.Amount,
                Date = transaction.Date ?? DateTime.Today,
                WalletId = transaction.WalletId ?? default
            });
        }

        [GraphQLNonNullType]
        [Authorize(Policy = AuthOptions.AUTH_POLICY)]
        public Task<Transaction> UpdateTransaction(
            [Service] IContextProvider contextProvider,
            [GraphQLNonNullType] UpdateTransactionInput transaction)
        {
            return contextProvider.GetService<ITransactionService>().Update(new Transaction
            {
                Id = transaction.Id,
                Title = transaction.Title,
                Amount = transaction.Amount ?? default,
                Date = transaction.Date ?? DateTime.Today
            });
        }

        [GraphQLNonNullType]
        [Authorize(Policy = AuthOptions.AUTH_POLICY, Roles = new[] {AuthOptions.USER_ROLE})]
        public Task<Transaction> DeleteTransaction(
            [Service] IContextProvider contextProvider,
            [GraphQLNonNullType] int id)
        {
            return contextProvider.GetService<ITransactionService>().Delete(id);
        }
    }
}
