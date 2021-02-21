using System.Collections.Generic;
using System.Threading.Tasks;
using CashSchedulerWebServer.Auth.Contracts;
using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Exceptions;
using CashSchedulerWebServer.Models;
using CashSchedulerWebServer.Services.Contracts;
using CashSchedulerWebServer.WebServices.Contracts;

namespace CashSchedulerWebServer.Services.Wallets
{
    public class WalletService : IWalletService
    { 
        private IContextProvider ContextProvider { get; }
        private IExchangeRateWebService ExchangeRateWebService { get; }
        private int UserId { get; }

        public WalletService(
            IContextProvider contextProvider,
            IExchangeRateWebService exchangeRateWebService,
            IUserContext userContext)
        {
            ContextProvider = contextProvider;
            ExchangeRateWebService = exchangeRateWebService;
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
                IsDefault = true,
                IsCustom = false
            });
        }

        public Task<Wallet> Create(Wallet wallet)
        {
            wallet.User ??= ContextProvider.GetRepository<IUserRepository>().GetByKey(UserId);
            
            wallet.Currency ??= ContextProvider.GetRepository<ICurrencyRepository>()
                .GetByKey(wallet.CurrencyAbbreviation);

            if (wallet.Currency == null)
            {
                throw new CashSchedulerException("There is no such currency", new[] { "currencyAbbreviation" });
            }

            return ContextProvider.GetRepository<IWalletRepository>().Create(wallet);
        }

        public async Task<Wallet> Update(Wallet wallet)
        {
            var walletRepository = ContextProvider.GetRepository<IWalletRepository>();
            
            var targetWallet = walletRepository.GetByKey(wallet.Id);

            if (!string.IsNullOrEmpty(wallet.Name))
            {
                targetWallet.Name = wallet.Name;
            }

            if (!string.IsNullOrEmpty(wallet.CurrencyAbbreviation) && targetWallet.Currency.Abbreviation != wallet.CurrencyAbbreviation)
            {
                var convertCurrencyResponse = await ExchangeRateWebService.ConvertCurrency(
                    targetWallet.Currency.Abbreviation,
                    wallet.CurrencyAbbreviation,
                    targetWallet.Balance
                );
                
                if (convertCurrencyResponse.Success)
                {
                    targetWallet.Balance = convertCurrencyResponse.Result;
                }

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
            
            return await walletRepository.Update(targetWallet);
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
    }
}
