using CashSchedulerWebServer.Authentication.Contracts;
using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Types;
using GraphQL.Authorization;
using GraphQL.Types;
using Microsoft.Extensions.Configuration;
using System;

namespace CashSchedulerWebServer.Queries
{
    public class CashSchedulerQuery : ObjectGraphType
    {
        public CashSchedulerQuery(IContextProvider contextProvider, IAuthenticator authenticator, IConfiguration configuration)
        {
            string policy = configuration["App:Auth:UserPolicy"];

            // Users
            Field<UserType>("getUser", resolve: context => contextProvider.GetRepository<IUserRepository>().GetById()).AuthorizeWith(policy);
            Field<StringGraphType>(
                "checkEmail",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "email" }),
                resolve: context => authenticator.CheckEmail(context.GetArgument<string>("email"))
            );
            Field<StringGraphType>(
                "checkCode",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "email" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "code" }
                ),
                resolve: context => authenticator.CheckCode(context.GetArgument<string>("email"), context.GetArgument<string>("code"))
            );

            // TransactionTypes
            Field<ListGraphType<TransactionTypeType>>("getTransactionTypes", resolve: context => contextProvider.GetRepository<ITransactionTypeRepository>().GetAll());

            // Categories
            Field<ListGraphType<CategoryType>>(
                "getAllCategories",
                arguments: new QueryArguments(new QueryArgument<StringGraphType> { Name = "transactionType" }),
                resolve: context => contextProvider.GetRepository<ICategoryRepository>().GetAll(context.GetArgument<string>("transactionType"))
            ).AuthorizeWith(policy);
            Field<ListGraphType<CategoryType>>("getStandardCategories", resolve: context => contextProvider.GetRepository<ICategoryRepository>().GetStandardCategories());
            Field<ListGraphType<CategoryType>>("getCustomCategories", resolve: context => contextProvider.GetRepository<ICategoryRepository>().GetCustomCategories()).AuthorizeWith(policy);

            // Transactions
            Field<ListGraphType<TransactionType>>(
                "getAllTransactions",
                arguments: new QueryArguments(new QueryArgument<IntGraphType> { Name = "size", DefaultValue = 0 }),
                resolve: context => contextProvider.GetRepository<ITransactionRepository>().GetAll(context.GetArgument<int>("size"))
            ).AuthorizeWith(policy);
            Field<ListGraphType<TransactionType>>(
                "getTransactionsForLastDays",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<IntGraphType>> { Name = "days" }),
                resolve: context => contextProvider.GetRepository<ITransactionRepository>().GetTransactionsForLastDays(context.GetArgument<int>("days"))
            ).AuthorizeWith(policy);
            Field<ListGraphType<TransactionType>>(
                "getTransactionsByMonth",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<IntGraphType>> { Name = "month", DefaultValue = DateTime.Today.Month },
                    new QueryArgument<IntGraphType> { Name = "year", DefaultValue = DateTime.Today.Year }
                ),
                resolve: context => contextProvider.GetRepository<ITransactionRepository>().GetTransactionsByMonth(context.GetArgument<int>("month"), context.GetArgument<int>("year"))
            ).AuthorizeWith(policy);

            // RegularTransactions
            Field<ListGraphType<RegularTransactionType>>(
                "getAllRegularTransactions",
                arguments: new QueryArguments(new QueryArgument<IntGraphType> { Name = "size", DefaultValue = 0 }),
                resolve: context => contextProvider.GetRepository<IRegularTransactionRepository>().GetAll(context.GetArgument<int>("size"))
            ).AuthorizeWith(policy);

            // UserNotifications
            Field<ListGraphType<UserNotificationType>>("getAllNotifications", resolve: context => contextProvider.GetRepository<IUserNotificationRepository>().GetAll()).AuthorizeWith(policy); ;
            Field<ListGraphType<UserNotificationType>>("getUnreadNotifications", resolve: context => contextProvider.GetRepository<IUserNotificationRepository>().GetAllUnread()).AuthorizeWith(policy);

            // UserSettings
            Field<ListGraphType<UserSettingType>>(
                "getUserSettings",
                arguments: new QueryArguments(new QueryArgument<StringGraphType> { Name = "unitName" }),
                resolve: context => contextProvider.GetRepository<IUserSettingRepository>().GetAllByUnitName(context.GetArgument<string>("unitName"))
            ).AuthorizeWith(policy);
        }
    }
}
