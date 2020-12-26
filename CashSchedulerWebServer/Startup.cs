using CashSchedulerWebServer.Db;
using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Schemas;
using GraphiQl;
using GraphQL;
using GraphQL.Types;
using GraphQL.Validation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using GraphQL.Authorization;
using CashSchedulerWebServer.Authentication;
using CashSchedulerWebServer.Authentication.Contracts;
using CashSchedulerWebServer.Notifications.Contracts;
using CashSchedulerWebServer.Notifications;
using Quartz.Impl;
using CashSchedulerWebServer.Jobs.Transactions;
using Quartz.Spi;
using Quartz;
using CashSchedulerWebServer.Jobs;
using System.Collections.Generic;
using CashSchedulerWebServer.Jobs.Reporting;
using GraphQL.Server;
using CashSchedulerWebServer.Db.Repositories;
using GraphQL.NewtonsoftJson;

namespace CashSchedulerWebServer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            #region JSON parser settings
            services.AddControllers().AddNewtonsoftJson(options => options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore);
            #endregion

            #region CORS
            services.AddCors(options =>
            {
                options.AddPolicy(
                    "ReactClient", 
                    builder => builder.WithOrigins(GetClientEndpoint(Configuration)).AllowAnyMethod().AllowAnyHeader()
                );
            });
            #endregion

            #region Authorization & Authentication
            services.AddTransient<UserContextManager>();
            services.AddTransient<IAuthenticator, Authenticator>();
            services.AddTransient<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IAuthorizationEvaluator, AuthorizationEvaluator>();
            services.AddTransient<IValidationRule, AuthorizationValidationRule>();
            services.AddSingleton(config =>
            {
                var authSettings = new AuthorizationSettings();
                authSettings.AddPolicy(Configuration["App:Auth:UserPolicy"], p =>
                {
                    p.RequireClaim("Id");
                });
                return authSettings;
            });
            #endregion

            #region Database
            services.AddDbContext<CashSchedulerContext>(options => options.UseSqlServer(GetConnectionString(Configuration)));
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
            services.AddTransient<IDocumentExecuter, DocumentExecuter>();
            services.AddTransient<IDocumentWriter, DocumentWriter>();
            services.AddTransient<ISchema, CashSchedulerSchema>();
            services.AddGraphQL().AddGraphTypes(typeof(CashSchedulerSchema), ServiceLifetime.Transient).AddWebSockets();
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
            app.UseGraphQLWebSockets<ISchema>();
            app.UseGraphiQl();
            app.UseRouting();
            app.UseCors();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseHttpsRedirection();
            app.UseEndpoints(endpoints => endpoints.MapControllers());
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
