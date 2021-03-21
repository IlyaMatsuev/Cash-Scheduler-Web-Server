using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CashSchedulerWebServer.Auth.Contracts;
using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Events;
using CashSchedulerWebServer.Events.Contracts;
using CashSchedulerWebServer.Exceptions;
using CashSchedulerWebServer.Models;
using CashSchedulerWebServer.Services.Contracts;
using CashSchedulerWebServer.Utils;

namespace CashSchedulerWebServer.Services.Wallets
{
    public class WalletService : IWalletService
    {
        private IContextProvider ContextProvider { get; }
        private IEventManager EventManager { get; }
        private int UserId { get; }

        public WalletService(IContextProvider contextProvider, IUserContext userContext, IEventManager eventManager)
        {
            ContextProvider = contextProvider;
            EventManager = eventManager;
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
                throw new CashSchedulerException("There is no such currency", new[] {"currencyAbbreviation"});
            }

            var createdWallet = await ContextProvider.GetRepository<IWalletRepository>().Create(wallet);

            if (wallet.IsDefault)
            {
                await ResetDefault(wallet);
            }

            await EventManager.FireEvent(EventAction.RecordUpserted, createdWallet);

            return createdWallet;
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
                    throw new CashSchedulerException("There is no such currency", new[] {"currencyAbbreviation"});
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

            await EventManager.FireEvent(EventAction.RecordUpserted, updatedWallet);

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
                    throw new CashSchedulerException("There is no such currency", new[] {"currencyAbbreviation"});
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

            await EventManager.FireEvent(EventAction.RecordUpserted, updatedWallet);

            return updatedWallet;
        }

        public async Task<Wallet> UpdateBalance(
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

            var updatedWallet = await ContextProvider.GetRepository<IWalletRepository>().Update(wallet);

            await EventManager.FireEvent(EventAction.RecordUpserted, updatedWallet);

            return updatedWallet;
        }

        public async Task<IEnumerable<Wallet>> UpdateBalance(
            IEnumerable<Transaction> transactions,
            IEnumerable<Transaction> oldTransactions,
            bool isCreate = false,
            bool isUpdate = false,
            bool isDelete = false)
        {
            Dictionary<int, Wallet> walletsByIds = new Dictionary<int, Wallet>();
            foreach (var transaction in transactions)
            {
                var oldTransaction = oldTransactions.FirstOrDefault(t => t.Id == transaction.Id);
                int delta = 1;

                if (!walletsByIds.ContainsKey(transaction.Wallet.Id))
                {
                    walletsByIds.Add(transaction.Wallet.Id, transaction.Wallet);
                }

                var wallet = walletsByIds[transaction.Wallet.Id];

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
                else if (isUpdate && oldTransaction != null)
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
                else if (isDelete && oldTransaction != null)
                {
                    if (oldTransaction.Date <= DateTime.Today)
                    {
                        wallet.Balance -= oldTransaction.Amount * delta;
                    }
                }
            }

            var updatedWallets = await ContextProvider.GetRepository<IWalletRepository>()
                .Update(walletsByIds.Values.ToList());

            await EventManager.FireEvent(EventAction.RecordUpserted, updatedWallets);

            return updatedWallets;
        }

        public async Task<Wallet> Delete(int id)
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

            var deletedWallet = await walletRepository.Delete(id);

            await EventManager.FireEvent(EventAction.RecordDeleted, deletedWallet);

            return deletedWallet;
        }

        public async Task<Transfer> CreateTransfer(Transfer transfer)
        {
            var walletRepository = ContextProvider.GetRepository<IWalletRepository>();

            transfer.SourceWallet = walletRepository.GetByKey(transfer.SourceWalletId);
            if (transfer.SourceWallet == null)
            {
                throw new CashSchedulerException("There is no such wallet", new[] {"sourceWalletId"});
            }

            transfer.TargetWallet = walletRepository.GetByKey(transfer.TargetWalletId);
            if (transfer.TargetWallet == null)
            {
                throw new CashSchedulerException("There is no such wallet", new[] {"targetWalletId"});
            }

            ModelValidator.ValidateModelAttributes(transfer);

            transfer.SourceWallet.Balance -= transfer.Amount;

            if (transfer.SourceWallet.Balance < 0)
            {
                throw new CashSchedulerException("There is not enough money on the wallet", new[] {"amount"});
            }

            await walletRepository.Update(transfer.SourceWallet);

            transfer.TargetWallet.Balance += transfer.ExchangeRate * transfer.Amount;
            await walletRepository.Update(transfer.TargetWallet);

            return transfer;
        }


        private Task<Wallet> ResetDefault(Wallet wallet)
        {
            var walletRepository = ContextProvider.GetRepository<IWalletRepository>();
            var defaultWallet = walletRepository.GetDefault();
            defaultWallet.IsDefault = false;
            wallet.IsDefault = true;
            return walletRepository.Update(defaultWallet);
        }
    }
}
