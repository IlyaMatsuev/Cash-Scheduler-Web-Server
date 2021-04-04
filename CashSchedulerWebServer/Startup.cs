using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using CashSchedulerWebServer.Db;
using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Jobs;
using CashSchedulerWebServer.Jobs.Transactions;
using CashSchedulerWebServer.Notifications;
using CashSchedulerWebServer.Notifications.Contracts;
using CashSchedulerWebServer.Auth;
using CashSchedulerWebServer.Auth.AuthenticationHandlers;
using CashSchedulerWebServer.Auth.AuthorizationHandlers;
using CashSchedulerWebServer.Auth.Contracts;
using CashSchedulerWebServer.Events;
using CashSchedulerWebServer.Events.Contracts;
using CashSchedulerWebServer.Events.Salesforce;
using CashSchedulerWebServer.Events.UserEvents;
using CashSchedulerWebServer.Exceptions;
using CashSchedulerWebServer.Jobs.Contracts;
using CashSchedulerWebServer.Mutations;
using CashSchedulerWebServer.Mutations.Categories;
using CashSchedulerWebServer.Mutations.CurrencyExchangeRates;
using CashSchedulerWebServer.Mutations.Notifications;
using CashSchedulerWebServer.Mutations.RecurringTransactions;
using CashSchedulerWebServer.Mutations.Salesforce;
using CashSchedulerWebServer.Mutations.Settings;
using CashSchedulerWebServer.Mutations.Transactions;
using CashSchedulerWebServer.Mutations.Users;
using CashSchedulerWebServer.Mutations.Wallets;
using CashSchedulerWebServer.Queries;
using CashSchedulerWebServer.Queries.TransactionTypes;
using CashSchedulerWebServer.Queries.Categories;
using CashSchedulerWebServer.Queries.Currencies;
using CashSchedulerWebServer.Queries.CurrencyExchangeRates;
using CashSchedulerWebServer.Queries.RecurringTransactions;
using CashSchedulerWebServer.Queries.Salesforce;
using CashSchedulerWebServer.Queries.Transactions;
using CashSchedulerWebServer.Queries.UserNotifications;
using CashSchedulerWebServer.Queries.Users;
using CashSchedulerWebServer.Queries.UserSettings;
using CashSchedulerWebServer.Queries.Wallets;
using CashSchedulerWebServer.Services;
using CashSchedulerWebServer.Subscriptions;
using CashSchedulerWebServer.Subscriptions.Notifications;
using CashSchedulerWebServer.WebServices.Contracts;
using CashSchedulerWebServer.WebServices.ExchangeRates;
using CashSchedulerWebServer.WebServices.Salesforce;
using HotChocolate.AspNetCore;
using Microsoft.Extensions.FileProviders;

namespace CashSchedulerWebServer
{
    public class Startup
    {
        private IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }


        public void ConfigureServices(IServiceCollection services)
        {
            #region CORS
            
            services.AddCors(options => options.AddDefaultPolicy(builder =>
            {
                builder.WithOrigins(GetClientEndpoint(Configuration)).AllowAnyMethod().AllowAnyHeader();
            }));
            
            #endregion

            #region Authorization & Authentication
            
            services.AddTransient<IUserContext, UserContext>();
            services.AddTransient<IUserContextManager, UserContextManager>();
            services.AddTransient<IAuthService, AuthService>();
            services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();

            services.AddAuthentication("Default")
                .AddScheme<CashSchedulerAuthenticationOptions, CashSchedulerAuthenticationHandler>("Default", null);

            services.AddSingleton<IAuthorizationHandler, CashSchedulerAuthorizationHandler>();
            services.AddAuthorization(options =>
            {
                options.AddPolicy(
                    AuthOptions.AUTH_POLICY,
                    policy => policy.Requirements.Add(new CashSchedulerUserRequirement())
                );
            });

            #endregion

            #region Database
             
            services.AddDbContext<CashSchedulerContext>(options => options.UseSqlServer(GetConnectionString(Configuration)), ServiceLifetime.Transient);
            services.AddTransient<IContextProvider, ContextProvider>();

            #endregion

            #region Events

            services.AddScoped<IEventManager, EventManager>();
            services.AddScoped<IEventListener, CreateDefaultWalletListener>();
            services.AddScoped<IEventListener, CreateDefaultSettingsListener>();
            services.AddScoped<IEventListener, CreateSfContactListener>();
            services.AddScoped<IEventListener, UpsertSfRecordListener>();
            services.AddScoped<IEventListener, DeleteSfRecordListener>();
            services.AddScoped<IEventListener, LogUserLoginListener>();

            #endregion

            #region WebServices

            services.AddTransient<IExchangeRateWebService, ExchangeRateWebService>();
            services.AddTransient<ISalesforceApiWebService, SalesforceApiWebService>();

            #endregion

            #region Utils configurations
            
            services.AddSingleton<INotificator, Notificator>();
            
            #endregion

            #region Scheduling Jobs

            services.AddSingleton<IJobManager, JobManager>();
            services.AddSingleton<IJobFactory, JobFactory>();
            services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();
            services.AddTransient<TransactionsJob>();
            services.AddTransient<RecurringTransactionsJob>();
            services.AddSingleton(GetJobsList(Configuration));
            services.AddHostedService<CashSchedulerHostedService>();
            
            #endregion

            #region GraphQL

            services.AddGraphQLServer()
                .AddQueryType<Query>()
                    .AddTypeExtension<UserQueries>()
                    .AddTypeExtension<TransactionTypeQueries>()
                    .AddTypeExtension<CategoryQueries>()
                    .AddTypeExtension<TransactionQueries>()
                    .AddTypeExtension<RecurringTransactionQueries>()
                    .AddTypeExtension<UserNotificationQueries>()
                    .AddTypeExtension<UserSettingQueries>()
                    .AddTypeExtension<WalletQueries>()
                    .AddTypeExtension<CurrencyQueries>()
                    .AddTypeExtension<CurrencyExchangeRateQueries>()
                    .AddTypeExtension<SalesforceQueries>()
                .AddMutationType<Mutation>()
                    .AddTypeExtension<UserMutations>()
                    .AddTypeExtension<CategoryMutations>()
                    .AddTypeExtension<TransactionMutations>()
                    .AddTypeExtension<RecurringTransactionMutations>()
                    .AddTypeExtension<NotificationMutations>()
                    .AddTypeExtension<SettingMutations>()
                    .AddTypeExtension<WalletMutations>()
                    .AddTypeExtension<CurrencyExchangeRateMutations>()
                    .AddTypeExtension<SalesforceMutations>()
                .AddSubscriptionType<Subscription>()
                    .AddTypeExtension<UserNotificationSubscriptions>()
                .AddAuthorization()
                .AddInMemorySubscriptions()
                .AddErrorFilter<CashSchedulerErrorFilter>();

            #endregion
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (bool.Parse(Configuration["App:Db:Refresh"]))
            {
                CashSchedulerSeeder.InitializeDb(app, Configuration);
            }

            app.UseStaticFiles(GetStaticFileOptions(Configuration));
            app.UseWebSockets();
            app.UseRouting();
            app.UseCors();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UsePlayground(Configuration["App:Server:GraphQLAPIPath"], Configuration["App:Server:GraphQLPlaygroundPath"]);
            app.UseHttpsRedirection();
            app.UseEndpoints(endpoints => endpoints.MapGraphQL(Configuration["App:Server:GraphQLAPIPath"]));
        }


        private string GetClientEndpoint(IConfiguration configuration)
        {
            string protocol = configuration["App:Client:Protocol"];
            string host = configuration["App:Client:Host"];
            string port = configuration["App:Client:Port"];

            string endpoint = $"{protocol}://{host}";

            if (!string.IsNullOrEmpty(port))
            {
                endpoint += $":{port}";
            }

            return endpoint;
        }

        private string GetConnectionString(IConfiguration configuration)
        {
            string host = configuration["App:Db:Host"];
            string port = configuration["App:Db:Port"];
            string database = configuration["App:Db:Name"];
            string username = configuration["App:Db:Username"];
            string password = configuration["App:Db:Password"];
            string additionalOptions = "MultipleActiveResultSets=True";

            string connectionFromSecrets = configuration.GetConnectionString("Default");

            return string.IsNullOrEmpty(connectionFromSecrets)
                ? $"Server={host},{port};Initial Catalog={database};User ID={username};Password={password};{additionalOptions}"
                : $"{connectionFromSecrets};{additionalOptions}";
        }

        private List<JobMetadata> GetJobsList(IConfiguration configuration)
        {
            return new()
            {
                new JobMetadata(
                    typeof(TransactionsJob),
                    configuration["App:Jobs:Transactions:Name"],
                    configuration["App:Jobs:Transactions:Cron"]),
                new JobMetadata(
                    typeof(RecurringTransactionsJob),
                    configuration["App:Jobs:RecurringTransactions:Name"],
                    configuration["App:Jobs:RecurringTransactions:Cron"])
            };
        }

        private StaticFileOptions GetStaticFileOptions(IConfiguration configuration)
        {
            return new()
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(Directory.GetCurrentDirectory(),
                    configuration["App:Content:RootPath"])
                ),
                RequestPath = configuration["App:Content:RequestPath"]
            };
        }
    }
}
