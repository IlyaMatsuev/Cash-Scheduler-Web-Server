using System.Threading.Tasks;
using CashSchedulerWebServer.Auth;
using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Models;
using CashSchedulerWebServer.Services.Contracts;
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
        public Task<Wallet> CreateWallet(
            [Service] IContextProvider contextProvider,
            [GraphQLNonNullType] NewWalletInput wallet)
        {
            return contextProvider.GetService<IWalletService>().Create(new Wallet
            {
                Name = wallet.Name,
                Balance = wallet.Balance,
                CurrencyAbbreviation = wallet.CurrencyAbbreviation,
                IsDefault = wallet.IsDefault
            });
        }

        [GraphQLNonNullType]
        [Authorize(Policy = AuthOptions.AUTH_POLICY)]
        public Task<Wallet> UpdateWallet(
            [Service] IContextProvider contextProvider,
            [GraphQLNonNullType] UpdateWalletInput wallet)
        {
            return contextProvider.GetService<IWalletService>().Update(new Wallet
            {
                Id = wallet.Id,
                Name = wallet.Name,
                Balance = wallet.Balance ?? default,
                CurrencyAbbreviation = wallet.CurrencyAbbreviation,
                IsDefault = wallet.IsDefault
            }, wallet.ConvertBalance, wallet.ExchangeRate);
        }

        [GraphQLNonNullType]
        [Authorize(Policy = AuthOptions.AUTH_POLICY, Roles = new[] {AuthOptions.USER_ROLE})]
        public Task<Wallet> DeleteWallet([Service] IContextProvider contextProvider, [GraphQLNonNullType] int id)
        {
            return contextProvider.GetService<IWalletService>().Delete(id);
        }

        [GraphQLNonNullType]
        [Authorize(Policy = AuthOptions.AUTH_POLICY, Roles = new[] {AuthOptions.USER_ROLE})]
        public Task<Transfer> CreateTransfer(
            [Service] IContextProvider contextProvider,
            [GraphQLNonNullType] NewTransferInput transfer)
        {
            return contextProvider.GetService<IWalletService>().CreateTransfer(new Transfer
            {
                SourceWalletId = transfer.SourceWalletId,
                TargetWalletId = transfer.TargetWalletId,
                Amount = transfer.Amount,
                ExchangeRate = transfer.ExchangeRate
            });
        }
    }
}
