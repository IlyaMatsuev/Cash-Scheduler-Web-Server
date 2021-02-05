using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CashSchedulerWebServer.Auth.Contracts;
using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Exceptions;
using CashSchedulerWebServer.Models;
using CashSchedulerWebServer.Utils;
using CashSchedulerWebServer.WebServices.Contracts;
using Microsoft.EntityFrameworkCore;

namespace CashSchedulerWebServer.Db.Repositories
{
    public class WalletRepository : IWalletRepository 
    {
        private IContextProvider ContextProvider { get; }
        private CashSchedulerContext Context { get; }
        private IExchangeRateService ExchangeRateService { get; }
        private int UserId { get; }

        public WalletRepository(
            IContextProvider contextProvider,
            CashSchedulerContext context,
            IExchangeRateService exchangeRateService,
            IUserContext userContext)
        {
            ContextProvider = contextProvider;
            Context = context;
            ExchangeRateService = exchangeRateService;
            UserId = userContext.GetUserId();
        }
        
        
        public IEnumerable<Wallet> GetAll()
        {
            return Context.Wallets.Where(w => w.User.Id == UserId)
                .Include(w => w.User)
                .Include(w => w.Currency);
        }

        public Wallet GetById(int id)
        {
            return Context.Wallets.Where(w => w.Id == id && w.User.Id == UserId)
                .Include(w => w.User)
                .Include(w => w.Currency)
                .FirstOrDefault();
        }

        public async Task<Wallet> CreateDefault(User user)
        {
            var defaultCurrency = ContextProvider.GetRepository<ICurrencyRepository>().GetDefaultCurrency();

            return await Create(new Wallet
            {
                Name = "Default Wallet",
                Balance = user.Balance,
                Currency = defaultCurrency,
                User = user,
                IsDefault = true,
                IsCustom = false
            });
        }

        public async Task<Wallet> Create(Wallet wallet)
        {
            wallet.User ??= ContextProvider.GetRepository<IUserRepository>().GetById(UserId);
            
            wallet.Currency ??= ContextProvider.GetRepository<ICurrencyRepository>()
                .GetById(wallet.CurrencyAbbreviation);

            if (wallet.Currency == null)
            {
                throw new CashSchedulerException("There is no such currency", new[] { "currencyAbbreviation" });
            }
            
            ModelValidator.ValidateModelAttributes(wallet);

            Context.Wallets.Add(wallet);
            await Context.SaveChangesAsync();

            return GetById(wallet.Id);
        }

        public async Task<Wallet> Update(Wallet wallet)
        {
            var targetWallet = GetById(wallet.Id);

            if (!string.IsNullOrEmpty(wallet.Name))
            {
                targetWallet.Name = wallet.Name;
            }

            if (!string.IsNullOrEmpty(wallet.CurrencyAbbreviation) && targetWallet.Currency.Abbreviation != wallet.CurrencyAbbreviation)
            {
                var convertCurrencyResponse = await ExchangeRateService.ConvertCurrency(
                    targetWallet.Currency.Abbreviation,
                    wallet.CurrencyAbbreviation,
                    targetWallet.Balance
                );
                
                if (convertCurrencyResponse.Success)
                {
                    targetWallet.Balance = convertCurrencyResponse.Result;
                }

                targetWallet.Currency = ContextProvider.GetRepository<ICurrencyRepository>()
                    .GetById(wallet.CurrencyAbbreviation);
                
                if (targetWallet.Currency == null)
                {
                    throw new CashSchedulerException("There is no such currency", new[] { "currencyAbbreviation" });
                }
            }
            
            if (wallet.Balance != default)
            {
                targetWallet.Balance = wallet.Balance;
            }

            ModelValidator.ValidateModelAttributes(targetWallet);

            Context.Wallets.Update(targetWallet);
            await Context.SaveChangesAsync();

            return targetWallet;
        }

        public async Task<Wallet> Delete(int id)
        {
            var wallet = GetById(id);
            if (wallet == null)
            {
                throw new CashSchedulerException("There is no such wallet");
            }

            if (wallet.IsDefault)
            {
                throw new CashSchedulerException("Default wallet cannot be deleted");
            }

            Context.Wallets.Remove(wallet);
            await Context.SaveChangesAsync();

            return wallet;
        }
    }
}
