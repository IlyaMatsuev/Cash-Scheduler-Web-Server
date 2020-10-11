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
            // JSON request settings
            services.AddControllers().AddNewtonsoftJson(options => options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore);

            // Authorization & Authentication section
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

            // DataBase section
            services.AddTransient<IContextProvider, ContextProvider>();
            services.AddDbContext<CashSchedulerContext>(options => options.UseSqlServer(Configuration.GetConnectionString("Default")));

            // GraphQL configuration section
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
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, CashSchedulerContext db)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();

                if (bool.Parse(Configuration["Data:RefreshDataOnLaunch"]))
                {
                    db.InitializeDb();
                }
            }

            app.UseGraphiQl();
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseHttpsRedirection();
            app.UseEndpoints(endpoints => endpoints.MapControllers());   
        }
    }
}
