using System;
using System.Threading.Tasks;
using CashSchedulerWebServer.Auth.Contracts;
using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Exceptions;
using CashSchedulerWebServer.Models;
using CashSchedulerWebServer.Services.Contracts;
using CashSchedulerWebServer.Utils;
using Microsoft.Extensions.Configuration;

namespace CashSchedulerWebServer.Services.Users
{
    public class UserService : IUserService
    {
        private IConfiguration Configuration { get; }
        private IContextProvider ContextProvider { get; }
        private int UserId { get; }

        public UserService(IConfiguration configuration, IContextProvider contextProvider, IUserContext userContext)
        {
            Configuration = configuration;
            ContextProvider = contextProvider;
            UserId = userContext.GetUserId();
        }
        
        
        public User GetById()
        {
            return ContextProvider.GetRepository<IUserRepository>().GetById(UserId);
        }

        public User GetByEmail(string email)
        {
            return ContextProvider.GetRepository<IUserRepository>().GetUserByEmail(email);
        }

        public bool HasWithEmail(string email)
        {
            return ContextProvider.GetRepository<IUserRepository>().HasUserWithEmail(email);
        }

        public Task<User> Create(User user)
        {
            ModelValidator.ValidateModelAttributes(user);

            // TODO: try to create a new NotMapped field in the model and move validation to the repository
            user.Password = user.Password.Hash(Configuration);
            
            return ContextProvider.GetRepository<IUserRepository>().Create(user);
        }

        public Task<User> UpdatePassword(string email, string password)
        {
            var userRepository = ContextProvider.GetRepository<IUserRepository>();
            
            var user = userRepository.GetUserByEmail(email);
            if (user == null)
            {
                throw new CashSchedulerException("There is no such user", new[] { nameof(email) });
            }

            ModelValidator.ValidateModelAttributes(user);

            // TODO: try to create a new NotMapped field in the model and move validation to the repository
            user.Password = password.Hash(Configuration);

            return userRepository.Update(user);
        }

        public Task<User> Update(User user)
        {
            var userRepository = ContextProvider.GetRepository<IUserRepository>();
            
            var targetUser = userRepository.GetById(user.Id);
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
                targetUser.Balance = user.Balance;
            }

            ModelValidator.ValidateModelAttributes(targetUser);
            
            return userRepository.Update(targetUser);
        }

        public Task<User> UpdateBalance(
            Transaction transaction,
            Transaction oldTransaction,
            bool isCreate = false,
            bool isUpdate = false,
            bool isDelete = false)
        {
            int delta = 1;
            var user = transaction.User;

            if (transaction.Category.Type.Name == TransactionType.Options.Expense.ToString())
            {
                delta = -1;
            }

            if (isCreate)
            {
                if (transaction.Date <= DateTime.Today)
                {
                    user.Balance += transaction.Amount * delta;
                }
            }
            else if (isUpdate)
            {
                if (transaction.Date <= DateTime.Today && oldTransaction.Date <= DateTime.Today)
                {
                    user.Balance += (transaction.Amount - oldTransaction.Amount) * delta;
                }
                else if (transaction.Date <= DateTime.Today && oldTransaction.Date > DateTime.Today)
                {
                    user.Balance += transaction.Amount * delta;
                }
                else if (transaction.Date > DateTime.Today && oldTransaction.Date <= DateTime.Today)
                {
                    user.Balance -= oldTransaction.Amount * delta;
                }
            }
            else if (isDelete)
            {
                if (oldTransaction.Date <= DateTime.Today)
                {
                    user.Balance -= oldTransaction.Amount * delta;
                }
            }

            return ContextProvider.GetRepository<IUserRepository>().Update(user);
        }
        
        public Task<User> Delete(int id)
        {
            return ContextProvider.GetRepository<IUserRepository>().Delete(id);
        }
    }
}
