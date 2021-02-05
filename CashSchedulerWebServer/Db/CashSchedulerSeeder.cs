using CashSchedulerWebServer.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CashSchedulerWebServer.Db
{
    public static class CashSchedulerSeeder
    {
        public static void InitializeDb(IApplicationBuilder app, IConfiguration configuration)
        {
            using var serviceScope = app.ApplicationServices.CreateScope();
            var context = serviceScope.ServiceProvider.GetService<CashSchedulerContext>();

            context.Database.Migrate();

            using var dmlTransaction = context.Database.BeginTransaction();
            try
            {
                context
                    .EmptyDb()
                    .ResetIdentitiesSeed()
                    .SeedDb(configuration)
                    .CompleteTransaction();
            }
            catch (Exception error)
            {
                context.PreventTransaction(error);
            }
        }
        
        private static CashSchedulerContext EmptyDb(this CashSchedulerContext context)
        {
            Console.WriteLine("Deleting all records...");
            context.RegularTransactions.RemoveRange(context.RegularTransactions);
            context.Transactions.RemoveRange(context.Transactions);
            context.Categories.RemoveRange(context.Categories);
            context.TransactionTypes.RemoveRange(context.TransactionTypes);
            context.UserSettings.RemoveRange(context.UserSettings);
            context.UserNotifications.RemoveRange(context.UserNotifications);
            context.UserRefreshTokens.RemoveRange(context.UserRefreshTokens);
            context.Users.RemoveRange(context.Users);

            context.SaveChanges();

            return context;
        }
        
        private static CashSchedulerContext ResetIdentitiesSeed(this CashSchedulerContext context)
        {
            Console.WriteLine("Resetting tables' identities...");
            static string ResetTableIdentity(string tableName) => $"DBCC CHECKIDENT ('{tableName}', RESEED, 0);";

            context.Database.ExecuteSqlRaw(ResetTableIdentity(nameof(context.Users)));
            context.Database.ExecuteSqlRaw(ResetTableIdentity(nameof(context.UserRefreshTokens)));
            context.Database.ExecuteSqlRaw(ResetTableIdentity(nameof(context.UserSettings)));
            context.Database.ExecuteSqlRaw(ResetTableIdentity(nameof(context.UserNotifications)));
            context.Database.ExecuteSqlRaw(ResetTableIdentity(nameof(context.Categories)));
            context.Database.ExecuteSqlRaw(ResetTableIdentity(nameof(context.Transactions)));
            context.Database.ExecuteSqlRaw(ResetTableIdentity(nameof(context.RegularTransactions)));

            context.SaveChanges();

            return context;
        }

        private static CashSchedulerContext SeedDb(this CashSchedulerContext context, IConfiguration configuration)
        {
            Console.WriteLine("Loading data...");

            string mockDataFolderPath = GetMockDataFolderPath(configuration);

            string usersJson = File.ReadAllText(mockDataFolderPath + @"Users.json");
            string transactionTypesJson = File.ReadAllText(mockDataFolderPath + @"TransactionTypes.json");
            string userSettingsJson = File.ReadAllText(mockDataFolderPath + @"UserSettings.json");
            string userNotificationsJson = File.ReadAllText(mockDataFolderPath + @"UserNotifications.json");
            string categoriesJson = File.ReadAllText(mockDataFolderPath + @"Categories.json");
            string transactionsJson = File.ReadAllText(mockDataFolderPath + @"Transactions.json");
            string regularTransactionsJson = File.ReadAllText(mockDataFolderPath + @"RegularTransactions.json");
            string currenciesJson = File.ReadAllText(mockDataFolderPath + @"Currencies.json");

            var users = JsonConvert.DeserializeObject<List<User>>(usersJson);
            var transactionTypes = JsonConvert.DeserializeObject<List<TransactionType>>(transactionTypesJson);
            var userSettings = JsonConvert.DeserializeObject<List<UserSetting>>(userSettingsJson);
            var userNotifications = JsonConvert.DeserializeObject<List<UserNotification>>(userNotificationsJson);
            var categories = JsonConvert.DeserializeObject<List<Category>>(categoriesJson);
            var transactions = JsonConvert.DeserializeObject<List<Transaction>>(transactionsJson);
            var regularTransactions = JsonConvert.DeserializeObject<List<RegularTransaction>>(regularTransactionsJson);
            var currencies = JsonConvert.DeserializeObject<List<Currency>>(currenciesJson);

            context.Users.AddRange(users);
            context.SaveChanges();
            context.TransactionTypes.AddRange(transactionTypes);
            context.Currencies.AddRange(currencies);
            context.SaveChanges();

            categories.ForEach(category =>
            {
                category.Type = transactionTypes.FirstOrDefault(type => type.Name == category.Type.Name);
                if (category.IsCustom)
                {
                    category.User = users.FirstOrDefault(user => user.Id == category.User.Id);   
                    if (category.User != null && category.Type != null)
                    {
                        context.Categories.Add(category);
                    }
                }
                else
                {
                    if (category.Type != null)
                    {
                        context.Categories.Add(category);
                    }
                }
            });

            context.SaveChanges();


            userSettings.ForEach(setting =>
            {
                setting.User = users.FirstOrDefault(user => user.Id == setting.User.Id);
                if (setting.User != null)
                {
                    context.UserSettings.Add(setting);
                }
            });

            context.SaveChanges();

            userNotifications.ForEach(notification =>
            {
                notification.User = users.FirstOrDefault(user => user.Id == notification.User.Id);
                if (notification.User != null)
                {
                    context.UserNotifications.Add(notification);
                }
            });

            context.SaveChanges();

            transactions.ForEach(transaction =>
            {
                transaction.User = users.FirstOrDefault(user => user.Id == transaction.User.Id);
                transaction.Category = categories.FirstOrDefault(category => category.Id == transaction.Category.Id);
                if (transaction.User != null && transaction.Category != null)
                {
                    context.Transactions.Add(transaction);
                }
            });

            context.SaveChanges();

            regularTransactions.ForEach(transaction =>
            {
                transaction.User = users.FirstOrDefault(user => user.Id == transaction.User.Id);
                transaction.Category = categories.FirstOrDefault(category => category.Id == transaction.Category.Id);
                if (transaction.User != null && transaction.Category != null)
                {
                    context.RegularTransactions.Add(transaction);
                }
            });

            context.SaveChanges();

            return context;
        }

        private static void CompleteTransaction(this CashSchedulerContext context)
        {
            var transaction = context.Database.CurrentTransaction;
            if (transaction != null)
            {
                transaction.Commit();
                Console.WriteLine("Transaction has been completed");
            }
        }

        private static void PreventTransaction(this CashSchedulerContext context, Exception error)
        {
            var transaction = context.Database.CurrentTransaction;
            if (transaction != null)
            {
                transaction.Rollback();
                Console.WriteLine($"The error occured while loading the mock data: {error.Message}: \n{error.StackTrace}");
            }
        }


        private static string GetMockDataFolderPath(IConfiguration configuration)
        {
            return AppDomain.CurrentDomain.BaseDirectory + configuration["App:Db:MockDataRelativeFolderPath"];
        }
    }
}
