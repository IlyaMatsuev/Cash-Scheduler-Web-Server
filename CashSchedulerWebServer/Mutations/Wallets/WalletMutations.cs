﻿using System.Threading.Tasks;
using CashSchedulerWebServer.Auth;
using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Models;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Types;

namespace CashSchedulerWebServer.Mutations.Wallets
{
    [ExtendObjectType(Name = "Mutation")]
    public class WalletMutations
    {
        [GraphQLNonNullType]
        [Authorize(Policy = AuthOptions.AUTH_POLICY)]
        public Task<Wallet> CreateWallet([Service] IContextProvider contextProvider, [GraphQLNonNullType] NewWalletInput wallet)
        {
            return contextProvider.GetRepository<IWalletRepository>().Create(new Wallet
            {
                Name = wallet.Name,
                Balance = wallet.Balance,
                CurrencyAbbreviation = wallet.CurrencyAbbreviation,
                IsCustom = true
            });
        }

        [GraphQLNonNullType]
        [Authorize(Policy = AuthOptions.AUTH_POLICY)]
        public Task<Wallet> UpdateWallet([Service] IContextProvider contextProvider, [GraphQLNonNullType] UpdateWalletInput wallet)
        {
            return contextProvider.GetRepository<IWalletRepository>().Update(new Wallet
            {
                Id = wallet.Id,
                Name = wallet.Name,
                Balance = wallet.Balance ?? default,
                CurrencyAbbreviation = wallet.CurrencyAbbreviation
            });
        }

        [GraphQLNonNullType]
        [Authorize(Policy = AuthOptions.AUTH_POLICY)]
        public Task<Wallet> DeleteWallet([Service] IContextProvider contextProvider, [GraphQLNonNullType] int id)
        {
            return contextProvider.GetRepository<IWalletRepository>().Delete(id);
        }
    }
}
