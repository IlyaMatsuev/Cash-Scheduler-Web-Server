using CashSchedulerWebServer.Authentication.Contracts;
using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Models;
using CashSchedulerWebServer.Types;
using CashSchedulerWebServer.Types.Inputs;
using GraphQL.Authorization;
using GraphQL.Types;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace CashSchedulerWebServer.Mutations
{
    public class CashSchedulerMutation : ObjectGraphType
    {
        public CashSchedulerMutation(IAuthenticator authenticator, IContextProvider contextProvider, IConfiguration configuration)
        {
            string policy = configuration["App:Auth:UserPolicy"];

            #region Authorization
            // Authentication/Authorization
            Field<UserType>(
                "register",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<NewUserInputType>> { Name = "user" }),
                resolve: context => authenticator.Register(context.GetArgument<User>("user")));

            Field<AuthTokensType>(
                "login",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "email" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "password" }),
                resolve: context => authenticator.Login(context.GetArgument<string>("email"), context.GetArgument<string>("password")));

            Field<UserType>(
                "logout",
                resolve: context => authenticator.Logout()).AuthorizeWith(policy);

            Field<AuthTokensType>(
                "token",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "email" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "refreshToken" }),
                resolve: context => authenticator.Token(context.GetArgument<string>("email"), context.GetArgument<string>("refreshToken")));

            Field<UserType>(
                "resetPassword",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "email" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "code" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "password" }),
                resolve: context => authenticator.ResetPassword(context.GetArgument<string>("email"), context.GetArgument<string>("code"), context.GetArgument<string>("password")));
            #endregion

            #region Categories
            // Categories
            Field<CategoryType>(
                "createCategory",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<NewCategoryInputType>> { Name = "category" }),
                resolve: context => contextProvider.GetRepository<ICategoryRepository>().Create(context.GetArgument<Category>("category"))
            ).AuthorizeWith(policy);

            Field<CategoryType>(
                "updateCategory",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<UpdateCategoryInputType>> { Name = "category" }),
                resolve: context => contextProvider.GetRepository<ICategoryRepository>().Update(context.GetArgument<Category>("category"))
            ).AuthorizeWith(policy);

            Field<CategoryType>(
                "deleteCategory",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<IntGraphType>> { Name = "id" }),
                resolve: context => contextProvider.GetRepository<ICategoryRepository>().Delete(context.GetArgument<int>("id"))
            ).AuthorizeWith(policy);
            #endregion

            #region Transactions
            // Transactions
            Field<Types.TransactionType>(
                "createTransaction",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<NewTransactionInputType>> { Name = "transaction" }),
                resolve: context => contextProvider.GetRepository<ITransactionRepository>().Create(context.GetArgument<Transaction>("transaction"))
            ).AuthorizeWith(policy);

            Field<Types.TransactionType>(
                "updateTransaction",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<UpdateTransactionInputType>> { Name = "transaction" }),
                resolve: context => contextProvider.GetRepository<ITransactionRepository>().Update(context.GetArgument<Transaction>("transaction"))
            ).AuthorizeWith(policy);

            Field<Types.TransactionType>(
                "deleteTransaction",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<IntGraphType>> { Name = "id" }),
                resolve: context => contextProvider.GetRepository<ITransactionRepository>().Delete(context.GetArgument<int>("id"))
            ).AuthorizeWith(policy);
            #endregion

            #region Regular Transactions
            // Regular Transactions
            Field<RegularTransactionType>(
                "createRegularTransaction",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<NewRegularTransactionInputType>> { Name = "transaction" }),
                resolve: context => contextProvider.GetRepository<IRegularTransactionRepository>().Create(context.GetArgument<RegularTransaction>("transaction"))
            ).AuthorizeWith(policy);

            Field<RegularTransactionType>(
                "updateRegularTransaction",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<UpdateRegularTransactionInputType>> { Name = "transaction" }),
                resolve: context => contextProvider.GetRepository<IRegularTransactionRepository>().Update(context.GetArgument<RegularTransaction>("transaction"))
            ).AuthorizeWith(policy);

            Field<RegularTransactionType>(
                "deleteRegularTransaction",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<IntGraphType>> { Name = "id" }),
                resolve: context => contextProvider.GetRepository<IRegularTransactionRepository>().Delete(context.GetArgument<int>("id"))
            ).AuthorizeWith(policy);
            #endregion

            #region Notifications
            // Notifications
            Field<UserNotificationType>(
                "readNotification",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<IntGraphType>> { Name = "id" }),
                resolve: context => contextProvider.GetRepository<IUserNotificationRepository>().Read(context.GetArgument<int>("id"))
            ).AuthorizeWith(policy);
            #endregion

            #region Settings
            // Settings
            Field<UserSettingType>(
                "updateUserSetting",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<UpdateUserSettingInputType>> { Name = "setting" }),
                resolve: context => contextProvider.GetRepository<IUserSettingRepository>().Update(context.GetArgument<UserSetting>("setting"))
            ).AuthorizeWith(policy);

            Field<ListGraphType<UserSettingType>>(
                "updateUserSettings",
                arguments: new QueryArguments(new QueryArgument<ListGraphType<NonNullGraphType<UpdateUserSettingInputType>>> { Name = "settings" }),
                resolve: context => contextProvider.GetRepository<IUserSettingRepository>().Update(context.GetArgument<List<UserSetting>>("settings"))
            ).AuthorizeWith(policy);
            #endregion
        }
    }
}
