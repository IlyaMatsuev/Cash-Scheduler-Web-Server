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
using CashSchedulerWebServer.WebServices.Contracts;
using CashSchedulerWebServer.WebServices.Salesforce.SObjects;

namespace CashSchedulerWebServer.Db
{
    public static class CashSchedulerSeeder
    {
        public static void InitializeDb(IApplicationBuilder app, IConfiguration configuration)
        {
            using var serviceScope = app.ApplicationServices.CreateScope();
            var context = serviceScope.ServiceProvider.GetService<CashSchedulerContext>();
            var salesforceService = serviceScope.ServiceProvider.GetService<ISalesforceApiWebService>();

            context.Database.Migrate();

            using var dmlTransaction = context.Database.BeginTransaction();
            try
            {
                context
                    .EmptySfDb(salesforceService)
                    .EmptyDb()
                    .ResetIdentitiesSeed()
                    .SeedDb(configuration)
                    .SeedSfDb(salesforceService)
                    .CompleteTransaction();
            }
            catch (Exception error)
            {
                context.PreventTransaction(error);
            }
        }

        private static CashSchedulerContext EmptySfDb(
            this CashSchedulerContext context,
            ISalesforceApiWebService salesforceService)
        {
            Console.WriteLine("Deleting all salesforce records...");
            if (context.Categories.Any())
            {
                salesforceService.DeleteSObjects(
                    context.Categories.Select(t => new SfCategory(t.Id)).Cast<SfObject>().ToList()
                ).GetAwaiter().GetResult();
            }

            if (context.Transactions.Any())
            {
                salesforceService.DeleteSObjects(
                    context.Transactions.Select(t => new SfTransaction(t.Id)).Cast<SfObject>().ToList()
                ).GetAwaiter().GetResult();
            }

            if (context.RegularTransactions.Any())
            {
                salesforceService.DeleteSObjects(
                    context.RegularTransactions.Select(t => new SfRecurringTransaction(t.Id)).Cast<SfObject>().ToList()
                ).GetAwaiter().GetResult();
            }

            if (context.Users.Any())
            {
                salesforceService.DeleteSObjects(
                    context.Users.Select(u => new SfContact(u.Id)).Cast<SfObject>().ToList()
                ).GetAwaiter().GetResult();
            }
            return context;
        }

        private static CashSchedulerContext EmptyDb(this CashSchedulerContext context)
        {
            Console.WriteLine("Deleting all records...");
            context.RegularTransactions.RemoveRange(context.RegularTransactions);
            context.Transactions.RemoveRange(context.Transactions);
            context.Categories.RemoveRange(context.Categories);
            context.TransactionTypes.RemoveRange(context.TransactionTypes);
            context.Settings.RemoveRange(context.Settings);
            context.UserSettings.RemoveRange(context.UserSettings);
            context.UserNotifications.RemoveRange(context.UserNotifications);
            context.UserRefreshTokens.RemoveRange(context.UserRefreshTokens);
            context.UserEmailVerificationCodes.RemoveRange(context.UserEmailVerificationCodes);
            context.CurrencyExchangeRates.RemoveRange(context.CurrencyExchangeRates);
            context.Wallets.RemoveRange(context.Wallets);
            context.Currencies.RemoveRange(context.Currencies);
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
            context.Database.ExecuteSqlRaw(ResetTableIdentity(nameof(context.CurrencyExchangeRates)));
            context.Database.ExecuteSqlRaw(ResetTableIdentity(nameof(context.Wallets)));

            context.SaveChanges();

            return context;
        }

        private static CashSchedulerContext SeedDb(this CashSchedulerContext context, IConfiguration configuration)
        {
            Console.WriteLine("Loading data...");

            string mockDataFolderPath = GetMockDataFolderPath(configuration);

            string usersJson = File.ReadAllText(mockDataFolderPath + @"Users.json");
            string transactionTypesJson = File.ReadAllText(mockDataFolderPath + @"TransactionTypes.json");
            string settingsJson = File.ReadAllText(mockDataFolderPath + @"Settings.json");
            string userSettingsJson = File.ReadAllText(mockDataFolderPath + @"UserSettings.json");
            string userNotificationsJson = File.ReadAllText(mockDataFolderPath + @"UserNotifications.json");
            string categoriesJson = File.ReadAllText(mockDataFolderPath + @"Categories.json");
            string transactionsJson = File.ReadAllText(mockDataFolderPath + @"Transactions.json");
            string regularTransactionsJson = File.ReadAllText(mockDataFolderPath + @"RegularTransactions.json");
            string currenciesJson = File.ReadAllText(mockDataFolderPath + @"Currencies.json");
            string walletsJson = File.ReadAllText(mockDataFolderPath + @"Wallets.json");

            var users = JsonConvert.DeserializeObject<List<User>>(usersJson);
            var transactionTypes = JsonConvert.DeserializeObject<List<TransactionType>>(transactionTypesJson);
            var settings = JsonConvert.DeserializeObject<List<Setting>>(settingsJson);
            var userSettings = JsonConvert.DeserializeObject<List<UserSetting>>(userSettingsJson);
            var userNotifications = JsonConvert.DeserializeObject<List<UserNotification>>(userNotificationsJson);
            var categories = JsonConvert.DeserializeObject<List<Category>>(categoriesJson);
            var transactions = JsonConvert.DeserializeObject<List<Transaction>>(transactionsJson);
            var regularTransactions = JsonConvert.DeserializeObject<List<RegularTransaction>>(regularTransactionsJson);
            var currencies = JsonConvert.DeserializeObject<List<Currency>>(currenciesJson);
            var wallets = JsonConvert.DeserializeObject<List<Wallet>>(walletsJson);

            context.Users.AddRange(users);
            context.SaveChanges();
            context.TransactionTypes.AddRange(transactionTypes);
            context.Settings.AddRange(settings);
            context.Currencies.AddRange(currencies);
            context.SaveChanges();

            wallets.ForEach(wallet =>
            {
                wallet.User = users.FirstOrDefault(user => user.Id == wallet.User.Id);
                wallet.Currency = currencies.FirstOrDefault(c => c.Abbreviation == wallet.Currency.Abbreviation);
                if (wallet.User != null && wallet.Currency != null)
                {
                    context.Wallets.Add(wallet);
                }
            });
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
                setting.Setting = settings.FirstOrDefault(s => s.Name == setting.Setting.Name);
                if (setting.User != null && setting.Setting != null)
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
                transaction.Wallet = wallets.FirstOrDefault(wallet => wallet.Id == transaction.Wallet.Id);
                if (transaction.User != null && transaction.Category != null && transaction.Wallet != null)
                {
                    context.Transactions.Add(transaction);
                }
            });

            context.SaveChanges();

            regularTransactions.ForEach(transaction =>
            {
                transaction.User = users.FirstOrDefault(user => user.Id == transaction.User.Id);
                transaction.Category = categories.FirstOrDefault(category => category.Id == transaction.Category.Id);
                transaction.Wallet = wallets.FirstOrDefault(wallet => wallet.Id == transaction.Wallet.Id);
                if (transaction.User != null && transaction.Category != null && transaction.Wallet != null)
                {
                    context.RegularTransactions.Add(transaction);
                }
            });

            context.SaveChanges();

            return context;
        }

        private static CashSchedulerContext SeedSfDb(
            this CashSchedulerContext context,
            ISalesforceApiWebService salesforceService)
        {
            Console.WriteLine("Loading salesforce data...");
            var users = context.Users;
            var wallets = context.Wallets
                .Include(w => w.User).ToList();
            var categories = context.Categories
                .Include(c => c.User)
                .Include(c => c.Type).ToList();
            var transactions = context.Transactions
                .Include(t => t.User)
                .Include(t => t.Category)
                .Include(t => t.Wallet).ToList();
            var recurringTransactions = context.RegularTransactions
                .Include(t => t.User)
                .Include(t => t.Category)
                .Include(t => t.Wallet).ToList();

            salesforceService.UpsertSObjects(users.Select(u => new SfContact(u, u.Id)).Cast<SfObject>().ToList())
                .GetAwaiter().GetResult();

            salesforceService.UpsertSObjects(wallets.Select(w => new SfWallet(w, w.Id)).Cast<SfObject>().ToList())
                .GetAwaiter().GetResult();

            salesforceService.UpsertSObjects(categories.Select(c => new SfCategory(c, c.Id)).Cast<SfObject>().ToList())
                .GetAwaiter().GetResult();

            salesforceService.UpsertSObjects(transactions.Select(t => new SfTransaction(t, t.Id)).Cast<SfObject>().ToList())
                .GetAwaiter().GetResult();

            salesforceService.UpsertSObjects(recurringTransactions.Select(t => new SfRecurringTransaction(t, t.Id)).Cast<SfObject>().ToList())
                .GetAwaiter().GetResult();

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
            return Directory.GetCurrentDirectory() + configuration["App:Db:MockDataPath"];
        }
    }
}
