using System;
using System.Threading.Tasks;
using CashSchedulerWebServer.Auth.Contracts;
using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Events;
using CashSchedulerWebServer.Events.Contracts;
using CashSchedulerWebServer.Exceptions;
using CashSchedulerWebServer.Models;
using CashSchedulerWebServer.Services.Contracts;
using CashSchedulerWebServer.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CashSchedulerWebServer.Services.Users
{
    public class UserService : IUserService
    {
        private IContextProvider ContextProvider { get; }
        private IEventManager EventManager { get; }
        private IUserRepository UserRepository { get; }
        private IConfiguration Configuration { get; }
        private IServiceScopeFactory ServiceScopeFactory { get; }
        private int UserId { get; }

        public UserService(
            IContextProvider contextProvider,
            IConfiguration configuration,
            IUserContext userContext,
            IEventManager eventManager,
            IServiceScopeFactory serviceScopeFactory)
        {
            ContextProvider = contextProvider;
            EventManager = eventManager;
            UserRepository = contextProvider.GetRepository<IUserRepository>();
            Configuration = configuration;
            ServiceScopeFactory = serviceScopeFactory;
            UserId = userContext.GetUserId();
        }


        public User GetById()
        {
            return UserRepository.GetByKey(UserId);
        }

        public User GetByEmail(string email)
        {
            return UserRepository.GetByEmail(email);
        }

        public bool HasWithEmail(string email)
        {
            return UserRepository.HasWithEmail(email);
        }

        public Task<User> Create(User user)
        {
            ModelValidator.ValidateModelAttributes(user);

            user.Password = user.Password.Hash(Configuration);

            return UserRepository.Create(user);
        }

        public Task<User> UpdatePassword(string email, string password)
        {
            var user = UserRepository.GetByEmail(email);
            if (user == null)
            {
                throw new CashSchedulerException("There is no such user", new[] {nameof(email)});
            }

            user.Password = password.Hash(Configuration);

            return UserRepository.Update(user);
        }

        public async Task<User> Update(User user)
        {
            var targetUser = UserRepository.GetByKey(user.Id);
            if (targetUser == null)
            {
                throw new CashSchedulerException("There is no such user");
            }

            if (user.FirstName != null)
            {
                targetUser.FirstName = user.FirstName;
            }

            if (user.LastName != null)
            {
                targetUser.LastName = user.LastName;
            }

            if (user.Balance != default)
            {
                var defaultWallet = ContextProvider.GetRepository<IWalletRepository>().GetDefault();
                defaultWallet.Balance = user.Balance;
                await ContextProvider.GetService<IWalletService>().Update(defaultWallet);
            }

            var createdUser = await UserRepository.Update(targetUser);

            await EventManager.FireEvent(EventAction.RecordUpserted, createdUser);

            return createdUser;
        }

        public async Task<User> Delete(string password)
        {
            var user = GetById();
            if (user == null)
            {
                throw new CashSchedulerException("There is no such user");
            }

            if (user.Password != password.Hash(Configuration))
            {
                throw new CashSchedulerException("Password in not valid", new []{nameof(password)});
            }

            var settingRepository = ContextProvider.GetRepository<IUserSettingRepository>();

            var deleteAccountSetting = settingRepository.GetByName(Setting.SettingOptions.DeleteAccount.ToString());
            await settingRepository.Delete(deleteAccountSetting.Id);

            await Task.Run(() =>
            {
                Task.Delay(TimeSpan.FromMinutes(Convert.ToInt32(Configuration["App:DeleteAccountTimeout"])))
                    .ContinueWith(_ => Delete(user.Id));
            });

            return user;
        }

        public async Task<User> Delete(int id)
        {
            using var scope = ServiceScopeFactory.CreateScope();
            var contextProvider = (IContextProvider) scope.ServiceProvider.GetService(typeof(IContextProvider));
            User user = null;
            await using var transaction = await contextProvider.BeginTransaction();
            try
            {
                contextProvider.GetRepository<ICurrencyExchangeRateRepository>().DeleteByUserId(id);
                contextProvider.GetRepository<IUserNotificationRepository>().DeleteByUserId(id);
                contextProvider.GetRepository<IUserSettingRepository>().DeleteByUserId(id);
                contextProvider.GetRepository<IUserRefreshTokenRepository>().DeleteByUserId(id);
                contextProvider.GetRepository<IUserEmailVerificationCodeRepository>().DeleteByUserId(id);
                contextProvider.GetRepository<ITransactionRepository>().DeleteByUserId(id);
                contextProvider.GetRepository<IRegularTransactionRepository>().DeleteByUserId(id);
                contextProvider.GetRepository<ICategoryRepository>().DeleteByUserId(id);
                contextProvider.GetRepository<IWalletRepository>().DeleteByUserId(id);
                user = await contextProvider.GetRepository<IUserRepository>().Delete(id);
                await EventManager.FireEvent(EventAction.UserDeleted, user);
                await contextProvider.EndTransaction(transaction);
            }
            catch (Exception error)
            {
                Console.WriteLine(error.Message);
                Console.WriteLine(error.StackTrace);
                await contextProvider.PreventTransaction(transaction);
            }
            return user;
        }
    }
}
