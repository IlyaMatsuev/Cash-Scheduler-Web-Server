using CashSchedulerWebServer.Db;
using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Queries;
using CashSchedulerWebServer.Schemas;
using CashSchedulerWebServer.Types;
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
using CashSchedulerWebServer.Mutations;
using CashSchedulerWebServer.Types.Inputs;
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
                    builder => builder.WithOrigins("http://localhost:3000").AllowAnyMethod().AllowAnyHeader()
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
            services.AddTransient<IContextProvider, ContextProvider>();
            services.AddDbContext<CashSchedulerContext>(options => options.UseSqlServer(GetConnectionString(Configuration)));
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
            services.AddTransient<IDependencyResolver>(resolver => new FuncDependencyResolver(resolver.GetRequiredService));
            services.AddTransient<CashSchedulerQuery>();
            services.AddTransient<CashSchedulerMutation>();
            services.AddTransient<UserType>();
            services.AddTransient<AuthTokensType>();
            services.AddTransient<UserSettingType>();
            services.AddTransient<UserNotificationType>();
            services.AddTransient<CategoryType>();
            services.AddTransient<TransactionTypeType>();
            services.AddTransient<TransactionType>();
            services.AddTransient<RegularTransactionType>();

            services.AddTransient<NewUserInputType>();
            services.AddTransient<NewCategoryInputType>();
            services.AddTransient<UpdateCategoryInputType>();
            services.AddTransient<NewTransactionInputType>();
            services.AddTransient<UpdateTransactionInputType>();
            services.AddTransient<NewRegularTransactionInputType>();
            services.AddTransient<UpdateRegularTransactionInputType>();
            services.AddTransient<UpdateUserSettingInputType>();

            services.AddTransient<IDocumentExecuter, DocumentExecuter>();
            services.AddTransient<ISchema, CashSchedulerSchema>();
            #endregion
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            if (bool.Parse(Configuration["Data:RefreshDataOnLaunch"]))
            {
                CashSchedulerSeeder.InitializeDb(app);
            }

            app.UseGraphiQl();
            app.UseRouting();
            app.UseCors();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseHttpsRedirection();
            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }


        private string GetConnectionString(IConfiguration configuration)
        {
            string server = configuration["DbServer"] ?? ".\\SQLEXPRESS";
            string port = configuration["DbPort"] ?? "1433";
            string username = configuration["DbUsername"];
            string password = configuration["DbPassword"];
            string database = configuration["DbName"] ?? "cash_scheduler";

            return bool.Parse(configuration["App:Db:ConnectionStringFromSecrets"])
                ? configuration.GetConnectionString("Default")
                : $"Server={server},{port};Initial Catalog={database};User ID = {username};Password={password}";
        }
    }
}
