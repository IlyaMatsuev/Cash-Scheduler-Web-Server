using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CashSchedulerWebServer.Db.Repositories;
using CashSchedulerWebServer.Services.Categories;
using CashSchedulerWebServer.Services.Contracts;
using CashSchedulerWebServer.Services.Currencies;
using CashSchedulerWebServer.Services.Notifications;
using CashSchedulerWebServer.Services.Salesforce;
using CashSchedulerWebServer.Services.Settings;
using CashSchedulerWebServer.Services.Transactions;
using CashSchedulerWebServer.Services.TransactionTypes;
using CashSchedulerWebServer.Services.Users;
using CashSchedulerWebServer.Services.Wallets;
using Microsoft.EntityFrameworkCore.Storage;

namespace CashSchedulerWebServer.Db
{
    public class ContextProvider : IContextProvider
    {
        private IServiceProvider ServiceProvider { get; }
        private CashSchedulerContext Context { get; }

        private Dictionary<Type, Type> RepositoryTypesMap { get; } = new()
        {
            {typeof(IUserRepository), typeof(UserRepository)},
            {typeof(IUserNotificationRepository), typeof(UserNotificationRepository)},
            {typeof(IUserSettingRepository), typeof(UserSettingRepository)},
            {typeof(IUserRefreshTokenRepository), typeof(UserRefreshTokenRepository)},
            {typeof(IUserEmailVerificationCodeRepository), typeof(UserEmailVerificationCodeRepository)},
            {typeof(ISettingRepository), typeof(SettingRepository)},
            {typeof(ILanguageRepository), typeof(LanguageRepository)},
            {typeof(ITransactionTypeRepository), typeof(TransactionTypeRepository)},
            {typeof(ICategoryRepository), typeof(CategoryRepository)},
            {typeof(ITransactionRepository), typeof(TransactionRepository)},
            {typeof(IRegularTransactionRepository), typeof(RegularTransactionRepository)},
            {typeof(ICurrencyRepository), typeof(CurrencyRepository)},
            {typeof(ICurrencyExchangeRateRepository), typeof(CurrencyExchangeRateRepository)},
            {typeof(IWalletRepository), typeof(WalletRepository)}
        };
        
        private Dictionary<Type, Type> ServiceTypesMap { get; } = new()
        {
            {typeof(IUserService), typeof(UserService)},
            {typeof(IUserSettingService), typeof(UserSettingService)},
            {typeof(IUserNotificationService), typeof(UserNotificationService)},
            {typeof(IUserEmailVerificationCodeService), typeof(UserEmailVerificationCodeService)},
            {typeof(IUserRefreshTokenService), typeof(UserRefreshTokenService)},
            {typeof(ICategoryService), typeof(CategoryService)},
            {typeof(ITransactionTypeService), typeof(TransactionTypeService)},
            {typeof(ITransactionService), typeof(TransactionService)},
            {typeof(IRecurringTransactionService), typeof(RecurringTransactionService)},
            {typeof(ICurrencyService), typeof(CurrencyService)},
            {typeof(ICurrencyExchangeRateService), typeof(CurrencyExchangeRateService)},
            {typeof(IWalletService), typeof(WalletService)},
            {typeof(ISalesforceService), typeof(SalesforceService)}
        };

        public ContextProvider(IServiceProvider serviceProvider, CashSchedulerContext context)
        {
            ServiceProvider = serviceProvider;
            Context = context;
        }


        public T GetRepository<T>() where T : class => GetService<T>(RepositoryTypesMap);

        public T GetService<T>() where T : class => GetService<T>(ServiceTypesMap);

        public Task<IDbContextTransaction> BeginTransaction()
        {
            return Context.Database.BeginTransactionAsync();
        }

        public Task PreventTransaction(IDbContextTransaction transaction)
        {
            return transaction.RollbackAsync();
        }

        public Task EndTransaction(IDbContextTransaction transaction)
        {
            return transaction.CommitAsync();
        }


        private T GetService<T>(Dictionary<Type, Type> typesMap) where T : class
        {
            if (!typesMap.ContainsKey(typeof(T)))
            {
                throw new CashSchedulerException($"No services were registered for the type {typeof(T)}", "500");
            }
            
            var serviceType = typesMap[typeof(T)];

            var service = (T) Activator.CreateInstance(serviceType, GetServiceParams(serviceType));

            if (service == null)
            {
                throw new CashSchedulerException($"Couldn't create an instance for the type {typeof(T)}", "500");
            }

            return service;
        }

        private object[] GetServiceParams(Type repositoryType)
        {
            return repositoryType.GetConstructors()
                .First(c => c.GetParameters().Length == repositoryType.GetConstructors().Max(t => t.GetParameters().Length))
                .GetParameters().Select(p =>
                {
                    object param;

                    if (p.ParameterType == typeof(CashSchedulerContext))
                    {
                        param = Context;
                    }
                    else if (p.ParameterType == typeof(IContextProvider))
                    {
                        param = this;
                    }
                    else
                    {
                        param = ServiceProvider.GetService(p.ParameterType);
                    }

                    return param;
                }).ToArray();
        }
    }
}