using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CashSchedulerWebServer.Auth.Contracts;
using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Exceptions;
using CashSchedulerWebServer.Models;
using CashSchedulerWebServer.Services.Contracts;

namespace CashSchedulerWebServer.Services.Wallets
{
    public class WalletService : IWalletService
    { 
        private IContextProvider ContextProvider { get; }
        private int UserId { get; }

        public WalletService(IContextProvider contextProvider, IUserContext userContext)
        {
            ContextProvider = contextProvider;
            UserId = userContext.GetUserId();
        }
        
        
        public IEnumerable<Wallet> GetAll()
        {
            return ContextProvider.GetRepository<IWalletRepository>().GetAll();
        }

        public Task<Wallet> CreateDefault(User user)
        {
            var targetUser = ContextProvider.GetRepository<IUserRepository>().GetByKey(user.Id);
            
            var defaultCurrency = ContextProvider.GetRepository<ICurrencyRepository>().GetDefaultCurrency();

            return ContextProvider.GetRepository<IWalletRepository>().Create(new Wallet
            {
                Name = "Default Wallet",
                Balance = user.Balance,
                Currency = defaultCurrency,
                User = targetUser,
                IsDefault = true
            });
        }

        public async Task<Wallet> Create(Wallet wallet)
        {
            wallet.User ??= ContextProvider.GetRepository<IUserRepository>().GetByKey(UserId);

            wallet.Currency ??= ContextProvider.GetRepository<ICurrencyRepository>()
                .GetByKey(wallet.CurrencyAbbreviation);

            if (wallet.Currency == null)
            {
                throw new CashSchedulerException("There is no such currency", new[] { "currencyAbbreviation" });
            }

            var newWallet = await ContextProvider.GetRepository<IWalletRepository>().Create(wallet);

            if (wallet.IsDefault)
            {
                await ResetDefault(wallet);
            }

            return newWallet;
        }

        public async Task<Wallet> Update(Wallet wallet, bool convertBalance, float? exchangeRate = null)
        {
            var walletRepository = ContextProvider.GetRepository<IWalletRepository>();

            var targetWallet = walletRepository.GetByKey(wallet.Id);

            if (!string.IsNullOrEmpty(wallet.Name))
            {
                targetWallet.Name = wallet.Name;
            }

            if (!string.IsNullOrEmpty(wallet.CurrencyAbbreviation)
                && targetWallet.Currency.Abbreviation != wallet.CurrencyAbbreviation)
            {
                if (convertBalance && exchangeRate != null)
                {
                    targetWallet.Balance *= (double) exchangeRate;
                }

                targetWallet.Currency = ContextProvider.GetRepository<ICurrencyRepository>()
                    .GetByKey(wallet.CurrencyAbbreviation);
                
                if (targetWallet.Currency == null)
                {
                    throw new CashSchedulerException("There is no such currency", new[] { "currencyAbbreviation" });
                }
            }
            
            if (!convertBalance && wallet.Balance != default)
            {
                targetWallet.Balance = wallet.Balance;
            }

            var updatedWallet = await walletRepository.Update(targetWallet);

            if (wallet.IsDefault)
            {
                await ResetDefault(targetWallet);
            }

            return updatedWallet;
        }

        public async Task<Wallet> Update(Wallet wallet)
        {
            var walletRepository = ContextProvider.GetRepository<IWalletRepository>();

            var targetWallet = walletRepository.GetByKey(wallet.Id);

            if (!string.IsNullOrEmpty(wallet.Name))
            {
                targetWallet.Name = wallet.Name;
            }

            if (!string.IsNullOrEmpty(wallet.CurrencyAbbreviation)
                && targetWallet.Currency.Abbreviation != wallet.CurrencyAbbreviation)
            {
                targetWallet.Currency = ContextProvider.GetRepository<ICurrencyRepository>()
                    .GetByKey(wallet.CurrencyAbbreviation);
                
                if (targetWallet.Currency == null)
                {
                    throw new CashSchedulerException("There is no such currency", new[] { "currencyAbbreviation" });
                }
            }

            if (wallet.Balance != default)
            {
                targetWallet.Balance = wallet.Balance;
            }

            var updatedWallet = await walletRepository.Update(targetWallet);

            if (wallet.IsDefault)
            {
                await ResetDefault(targetWallet);
            }

            return updatedWallet;
        }

        public Task<Wallet> UpdateBalance(
            Transaction transaction,
            Transaction oldTransaction,
            bool isCreate = false,
            bool isUpdate = false,
            bool isDelete = false)
        {
            int delta = 1;
            var wallet = transaction.Wallet;

            if (transaction.Category.Type.Name == TransactionType.Options.Expense.ToString())
            {
                delta = -1;
            }

            if (isCreate)
            {
                if (transaction.Date <= DateTime.Today)
                {
                    wallet.Balance += transaction.Amount * delta;
                }
            }
            else if (isUpdate)
            {
                if (transaction.Date <= DateTime.Today && oldTransaction.Date <= DateTime.Today)
                {
                    wallet.Balance += (transaction.Amount - oldTransaction.Amount) * delta;
                }
                else if (transaction.Date <= DateTime.Today && oldTransaction.Date > DateTime.Today)
                {
                    wallet.Balance += transaction.Amount * delta;
                }
                else if (transaction.Date > DateTime.Today && oldTransaction.Date <= DateTime.Today)
                {
                    wallet.Balance -= oldTransaction.Amount * delta;
                }
            }
            else if (isDelete)
            {
                if (oldTransaction.Date <= DateTime.Today)
                {
                    wallet.Balance -= oldTransaction.Amount * delta;
                }
            }

            return ContextProvider.GetRepository<IWalletRepository>().Update(wallet);
        }

        public Task<Wallet> Delete(int id)
        {
            var walletRepository = ContextProvider.GetRepository<IWalletRepository>();
            
            var wallet = walletRepository.GetByKey(id);
            if (wallet == null)
            {
                throw new CashSchedulerException("There is no such wallet");
            }

            if (wallet.IsDefault)
            {
                throw new CashSchedulerException("Default wallet cannot be deleted");
            }
            
            return walletRepository.Delete(id);
        }


        private Task<Wallet> ResetDefault(Wallet newDefault)
        {
            var walletRepository = ContextProvider.GetRepository<IWalletRepository>();
            var defaultWallet = walletRepository.GetDefault();
            defaultWallet.IsDefault = false;
            newDefault.IsDefault = true;
            return walletRepository.Update(defaultWallet);
        }
    }
}
