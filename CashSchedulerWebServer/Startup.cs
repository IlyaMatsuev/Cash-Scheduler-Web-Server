using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using GraphiQl;
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
using CashSchedulerWebServer.Exceptions;
using CashSchedulerWebServer.Mutations;
using CashSchedulerWebServer.Mutations.Categories;
using CashSchedulerWebServer.Mutations.Notifications;
using CashSchedulerWebServer.Mutations.RecurringTransactions;
using CashSchedulerWebServer.Mutations.Settings;
using CashSchedulerWebServer.Mutations.Transactions;
using CashSchedulerWebServer.Mutations.Users;
using CashSchedulerWebServer.Queries;
using CashSchedulerWebServer.Queries.TransactionTypes;
using CashSchedulerWebServer.Queries.Categories;
using CashSchedulerWebServer.Queries.RecurringTransactions;
using CashSchedulerWebServer.Queries.Transactions;
using CashSchedulerWebServer.Queries.UserNotifications;
using CashSchedulerWebServer.Queries.Users;
using CashSchedulerWebServer.Queries.UserSettings;

namespace CashSchedulerWebServer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        private IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            #region JSON parser settings
            
            services.AddControllers().AddNewtonsoftJson(options => options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore);
            
            #endregion

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
            
            // TODO: choose the correct lifetime
            services.AddDbContext<CashSchedulerContext>(options => options.UseSqlServer(GetConnectionString(Configuration))/*, ServiceLifetime.Transient*/);
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
            
            #endregion

            #region Utils configurations
            
            services.AddSingleton<INotificator, Notificator>();
            
            #endregion

            #region Scheduling Jobs
            
            services.AddSingleton<IJobFactory, JobFactory>();
            services.AddSingleton<ISchedulerFactory, StdSchedulerFactory>();
            services.AddTransient<TransactionsJob>();
            services.AddTransient<RecurringTransactionsJob>();
            services.AddTransient<RecurringTransactionsJob>();
            services.AddSingleton(new List<JobMetadata>
            {
                new JobMetadata(typeof(TransactionsJob), Configuration["App:Jobs:Transactions:Name"], Configuration["App:Jobs:Transactions:Cron"]),
                new JobMetadata(typeof(RecurringTransactionsJob), Configuration["App:Jobs:RecurringTransactions:Name"], Configuration["App:Jobs:RecurringTransactions:Cron"]),
                new JobMetadata(typeof(ReportingJob), Configuration["App:Jobs:Reporting:Name"], Configuration["App:Jobs:Reporting:Cron"])
            });
            services.AddHostedService<TransactionsHostedService>();
            
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
                .AddMutationType<Mutation>()
                    .AddTypeExtension<UserMutations>()
                    .AddTypeExtension<CategoryMutations>()
                    .AddTypeExtension<TransactionMutations>()
                    .AddTypeExtension<RecurringTransactionMutations>()
                    .AddTypeExtension<NotificationMutations>()
                    .AddTypeExtension<SettingMutations>()
                .AddAuthorization()
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

            app.UseWebSockets();
            app.UseRouting();
            app.UseCors();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseGraphiQl(Configuration["App:Server:GraphiQLPath"], Configuration["App:Server:GraphQLAPIPath"]);
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
    }
}
