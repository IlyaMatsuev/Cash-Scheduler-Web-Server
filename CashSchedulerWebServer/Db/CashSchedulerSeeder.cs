﻿using CashSchedulerWebServer.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
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
        private static readonly string MOCK_DATA_FOLDER_PATH = AppDomain.CurrentDomain.BaseDirectory + "/../../../Db/MockData/";


        public static void InitializeDb(IApplicationBuilder app)
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
                    .SeedDb()
                    .CompleteTransaction();
            }
            catch (Exception error)
            {
                context.PreventTransaction(error);
            }
        }

        private static CashSchedulerContext SeedDb(this CashSchedulerContext context)
        {
            Console.WriteLine("Loading data...");

            string usersJson = File.ReadAllText(MOCK_DATA_FOLDER_PATH + @"Users.json");
            string transactionTypesJson = File.ReadAllText(MOCK_DATA_FOLDER_PATH + @"TransactionTypes.json");
            string userSettingsJson = File.ReadAllText(MOCK_DATA_FOLDER_PATH + @"UserSettings.json");
            string userNotificationsJson = File.ReadAllText(MOCK_DATA_FOLDER_PATH + @"UserNotifications.json");
            string categoriesJson = File.ReadAllText(MOCK_DATA_FOLDER_PATH + @"Categories.json");
            string transactionsJson = File.ReadAllText(MOCK_DATA_FOLDER_PATH + @"Transactions.json");
            string regularTransactionsJson = File.ReadAllText(MOCK_DATA_FOLDER_PATH + @"RegularTransactions.json");

            var users = JsonConvert.DeserializeObject<List<User>>(usersJson);
            var transactionTypes = JsonConvert.DeserializeObject<List<TransactionType>>(transactionTypesJson);
            var userSettings = JsonConvert.DeserializeObject<List<UserSetting>>(userSettingsJson);
            var userNotifications = JsonConvert.DeserializeObject<List<UserNotification>>(userNotificationsJson);
            var categories = JsonConvert.DeserializeObject<List<Category>>(categoriesJson);
            var transactions = JsonConvert.DeserializeObject<List<Transaction>>(transactionsJson);
            var regularTransactions = JsonConvert.DeserializeObject<List<RegularTransaction>>(regularTransactionsJson);

            context.Users.AddRange(users);
            context.SaveChanges();
            context.TransactionTypes.AddRange(transactionTypes);
            context.SaveChanges();

            categories.ForEach(category =>
            {
                category.Type = transactionTypes.FirstOrDefault(type => type.Name == category.Type.Name);
                if (category.IsCustom)
                {
                    category.CreatedBy = users.FirstOrDefault(user => user.Id == category.CreatedBy.Id);   
                    if (category.CreatedBy != null && category.Type != null)
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
                setting.SettingFor = users.FirstOrDefault(user => user.Id == setting.SettingFor.Id);
                if (setting.SettingFor != null)
                {
                    context.UserSettings.Add(setting);
                }
            });

            context.SaveChanges();

            userNotifications.ForEach(notification =>
            {
                notification.CreatedFor = users.FirstOrDefault(user => user.Id == notification.CreatedFor.Id);
                if (notification.CreatedFor != null)
                {
                    context.UserNotifications.Add(notification);
                }
            });

            context.SaveChanges();

            transactions.ForEach(transaction =>
            {
                transaction.CreatedBy = users.FirstOrDefault(user => user.Id == transaction.CreatedBy.Id);
                transaction.TransactionCategory = categories.FirstOrDefault(category => category.Id == transaction.TransactionCategory.Id);
                if (transaction.CreatedBy != null && transaction.TransactionCategory != null)
                {
                    context.Transactions.Add(transaction);
                }
            });

            context.SaveChanges();

            regularTransactions.ForEach(transaction =>
            {
                transaction.CreatedBy = users.FirstOrDefault(user => user.Id == transaction.CreatedBy.Id);
                transaction.TransactionCategory = categories.FirstOrDefault(category => category.Id == transaction.TransactionCategory.Id);
                if (transaction.CreatedBy != null && transaction.TransactionCategory != null)
                {
                    context.RegularTransactions.Add(transaction);
                }
            });

            context.SaveChanges();

            return context;
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
            Console.WriteLine("Resetting table's identities...");
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
    }
}
