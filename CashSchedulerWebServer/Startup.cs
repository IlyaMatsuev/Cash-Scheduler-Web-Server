using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using CashSchedulerWebServer.Db;
using CashSchedulerWebServer.Db.Repositories;
using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Jobs;
using CashSchedulerWebServer.Jobs.Reporting;
using CashSchedulerWebServer.Jobs.Transactions;
using CashSchedulerWebServer.Notifications;
using CashSchedulerWebServer.Notifications.Contracts;
using CashSchedulerWebServer.Auth;
using CashSchedulerWebServer.Auth.AuthenticationHandlers;
using CashSchedulerWebServer.Auth.AuthorizationHandlers;
using CashSchedulerWebServer.Auth.Contracts;
using CashSchedulerWebServer.Events;
using CashSchedulerWebServer.Events.Contracts;
using CashSchedulerWebServer.Events.UserEvents;
using CashSchedulerWebServer.Exceptions;
using CashSchedulerWebServer.Jobs.Contracts;
using CashSchedulerWebServer.Jobs.ExchangeRates;
using CashSchedulerWebServer.Mutations;
using CashSchedulerWebServer.Mutations.Categories;
using CashSchedulerWebServer.Mutations.CurrencyExchangeRates;
using CashSchedulerWebServer.Mutations.Notifications;
using CashSchedulerWebServer.Mutations.RecurringTransactions;
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
using CashSchedulerWebServer.Queries.Transactions;
using CashSchedulerWebServer.Queries.UserNotifications;
using CashSchedulerWebServer.Queries.Users;
using CashSchedulerWebServer.Queries.UserSettings;
using CashSchedulerWebServer.Queries.Wallets;
using CashSchedulerWebServer.Services;
using CashSchedulerWebServer.Services.Categories;
using CashSchedulerWebServer.Services.Contracts;
using CashSchedulerWebServer.Services.Currencies;
using CashSchedulerWebServer.Services.Notifications;
using CashSchedulerWebServer.Services.Settings;
using CashSchedulerWebServer.Services.Transactions;
using CashSchedulerWebServer.Services.TransactionTypes;
using CashSchedulerWebServer.Services.Users;
using CashSchedulerWebServer.Services.Wallets;
using CashSchedulerWebServer.Subscriptions;
using CashSchedulerWebServer.Subscriptions.Notifications;
using CashSchedulerWebServer.WebServices.Contracts;
using CashSchedulerWebServer.WebServices.ExchangeRates;
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
            services.AddTransient<IAuthenticator, Authenticator>();
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
            
            // TODO: I have to do something with my repositories because while executing a single mutation
            // I can instantiate several repositories which also have DbContext passed in 
            services.AddDbContext<CashSchedulerContext>(options => options.UseSqlServer(GetConnectionString(Configuration)), ServiceLifetime.Transient);
            services.AddTransient<IContextProvider, ContextProvider>();
            services.AddTransient<IUserRepository, UserRepository>();
            services.AddTransient<IUserEmailVerificationCodeRepository, UserEmailVerificationCodeRepository>();
            services.AddTransient<IUserRefreshTokenRepository, UserRefreshTokenRepository>();
            services.AddTransient<IUserNotificationRepository, UserNotificationRepository>();
            services.AddTransient<IUserSettingRepository, UserSettingRepository>();
            services.AddTransient<ITransactionTypeRepository, TransactionTypeRepository>();
            services.AddTransient<ICategoryRepository, CategoryRepository>();
            services.AddTransient<ITransactionRepository, TransactionRepository>();
            services.AddTransient<IRegularTransactionRepository, RegularTransactionRepository>();
            services.AddTransient<ICurrencyRepository, CurrencyRepository>();
            services.AddTransient<ICurrencyExchangeRateRepository, CurrencyExchangeRateRepository>();
            services.AddTransient<IWalletRepository, WalletRepository>();

            #endregion

            #region Services

            services.AddTransient<IUserService, UserService>();
            services.AddTransient<IUserSettingService, UserSettingService>();
            services.AddTransient<IUserNotificationService, UserNotificationService>();
            services.AddTransient<IUserEmailVerificationCodeService, UserEmailVerificationCodeService>();
            services.AddTransient<IUserRefreshTokenService, UserRefreshTokenService>();
            services.AddTransient<ICategoryService, CategoryService>();
            services.AddTransient<ITransactionTypeService, TransactionTypeService>();
            services.AddTransient<ITransactionService, TransactionService>();
            services.AddTransient<IRecurringTransactionService, RecurringTransactionService>();
            services.AddTransient<IWalletService, WalletService>();
            services.AddTransient<ICurrencyService, CurrencyService>();
            services.AddTransient<ICurrencyExchangeRateService, CurrencyExchangeRateService>();

            #endregion

            #region Events

            services.AddSingleton<IEventManager, EventManager>();
            services.AddSingleton<IEventListener, CreateDefaultWalletListener>();

            #endregion

            #region WebServices

            services.AddTransient<IExchangeRateWebService, ExchangeRateWebService>();

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
            services.AddTransient<ReportingJob>();
            services.AddTransient<ExchangeRateJob>();
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
                .AddMutationType<Mutation>()
                    .AddTypeExtension<UserMutations>()
                    .AddTypeExtension<CategoryMutations>()
                    .AddTypeExtension<TransactionMutations>()
                    .AddTypeExtension<RecurringTransactionMutations>()
                    .AddTypeExtension<NotificationMutations>()
                    .AddTypeExtension<SettingMutations>()
                    .AddTypeExtension<WalletMutations>()
                    .AddTypeExtension<CurrencyExchangeRateMutations>()
                .AddSubscriptionType<Subscription>()
                    .AddTypeExtension<NotificationSubscriptions>()
                .AddAuthorization()
                .AddInMemorySubscriptions()
                .AddErrorFilter<CashSchedulerErrorFilter>();

            #endregion
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            if (bool.Parse(Configuration["App:Db:Refresh"]))
            {
                CashSchedulerSeeder.InitializeDb(app, Configuration);
            }

            app.UseStaticFiles(GetStaticFileOptions(Configuration, env.ContentRootPath));
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

            return $"{protocol}://{host}:{port}";
        }

        private string GetConnectionString(IConfiguration configuration)
        {
            string host = configuration["App:Db:Host"];
            string port = configuration["App:Db:Port"];
            string database = configuration["App:Db:Name"];
            string username = configuration["App:Db:Username"];
            string password = configuration["App:Db:Password"];

            string connectionFromSecrets = configuration.GetConnectionString("Default");

            return string.IsNullOrEmpty(connectionFromSecrets)
                ? $"Server={host},{port};Initial Catalog={database};User ID = {username};Password={password}"
                : connectionFromSecrets;
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
                    configuration["App:Jobs:RecurringTransactions:Cron"]),
                new JobMetadata(
                    typeof(ReportingJob),
                    configuration["App:Jobs:Reporting:Name"],
                    configuration["App:Jobs:Reporting:Cron"]),
                new JobMetadata(
                    typeof(ExchangeRateJob),
                    configuration["App:Jobs:ExchangeRates:Name"],
                    configuration["App:Jobs:ExchangeRates:Cron"])
            };
        }

        private StaticFileOptions GetStaticFileOptions(IConfiguration configuration, string projectFolderPath)
        {
            return new()
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(projectFolderPath, configuration["App:Content:RootPath"])),
                RequestPath = configuration["App:Content:RequestPath"]
            };
        }
    }
}
