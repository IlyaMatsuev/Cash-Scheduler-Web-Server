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

namespace CashSchedulerWebServer.Services.Users
{
    public class UserService : IUserService
    {
        private IContextProvider ContextProvider { get; }
        private IEventManager EventManager { get; }
        private IUserRepository UserRepository { get; }
        private IConfiguration Configuration { get; }
        private int UserId { get; }

        public UserService(
            IContextProvider contextProvider,
            IConfiguration configuration,
            IUserContext userContext,
            IEventManager eventManager)
        {
            ContextProvider = contextProvider;
            EventManager = eventManager;
            UserRepository = contextProvider.GetRepository<IUserRepository>();
            Configuration = configuration;
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

        public Task<User> Delete(int id)
        {
            return UserRepository.Delete(id);
        }
    }
}
