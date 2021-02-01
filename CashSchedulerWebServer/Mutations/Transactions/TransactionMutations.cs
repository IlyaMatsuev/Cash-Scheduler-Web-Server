﻿using System;
using System.Threading.Tasks;
using CashSchedulerWebServer.Auth;
using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Models;
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
        public Task<Transaction> CreateTransaction([Service] IContextProvider contextProvider, [GraphQLNonNullType] NewTransactionInput transaction)
        {
            return contextProvider.GetRepository<ITransactionRepository>().Create(new Transaction
            {
                Title = transaction.Title,
                CategoryId = transaction.CategoryId,
                Amount = transaction.Amount,
                Date = transaction.Date ?? DateTime.Today
            });
        }
        
        [GraphQLNonNullType]
        [Authorize(Policy = AuthOptions.AUTH_POLICY)]
        public Task<Transaction> UpdateTransaction([Service] IContextProvider contextProvider, [GraphQLNonNullType] UpdateTransactionInput transaction)
        {
            return contextProvider.GetRepository<ITransactionRepository>().Update(new Transaction
            {
                Id = transaction.Id,
                Title = transaction.Title,
                Amount = transaction.Amount ?? default,
                Date = transaction.Date ?? DateTime.Today
            });
        }
        
        [GraphQLNonNullType]
        [Authorize(Policy = AuthOptions.AUTH_POLICY)]
        public Task<Transaction> DeleteTransaction([Service] IContextProvider contextProvider, [GraphQLNonNullType] int id)
        {
            return contextProvider.GetRepository<ITransactionRepository>().Delete(id);
        }
    }
}