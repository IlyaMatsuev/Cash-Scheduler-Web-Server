using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Db.Repositories;
using CashSchedulerWebServer.Exceptions;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CashSchedulerWebServer.Db
{
    public class ContextProvider : IContextProvider
    {
        private CashSchedulerContext Context { get; set; }
        private IHttpContextAccessor HttpAccessor { get; set; }

        private static readonly Dictionary<Type, Type> RepositoryTypesMap = new Dictionary<Type, Type>
        {
            { typeof(IUserRepository), typeof(UserRepository) },
            { typeof(ITransactionTypeRepository), typeof(TransactionTypeRepository) },
            { typeof(ICategoryRepository), typeof(CategoryRepository) },
            { typeof(ITransactionRepository), typeof(TransactionRepository) },
            { typeof(IRegularTransactionRepository), typeof(RegularTransactionRepository) },
            { typeof(IUserNotificationRepository), typeof(UserNotificationRepository) },
            { typeof(IUserSettingRepository), typeof(UserSettingRepository) },
            { typeof(IUserRefreshTokenRepository), typeof(UserRefreshTokenRepository) },
            { typeof(IUserEmailVerificationCodeRepository), typeof(UserEmailVerificationCodeRepository) }

        };

        public ContextProvider(CashSchedulerContext context, IHttpContextAccessor httpAccessor)
        {
            Context = context;
            HttpAccessor = httpAccessor;
        }


        public T GetRepository<T>() where T : class
        {
            var repositoryType = RepositoryTypesMap[typeof(T)];
            int maxConstructorParams = repositoryType.GetConstructors().Max(c => c.GetParameters().Length);
            var constructorParams = new object[] { Context, HttpAccessor, this };

            var repository = (T)Activator.CreateInstance(repositoryType, constructorParams.Take(maxConstructorParams).ToArray());

            if (repository == null)
            {
                throw new CashSchedulerException("No mapping provided for the requested repository interface");
            }
            return repository;
        }
    }
}
