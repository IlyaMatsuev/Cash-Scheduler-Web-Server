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
        private IUserRepository UserRepository { get; }
        private IConfiguration Configuration { get; }
        private int UserId { get; }

        public UserService(IContextProvider contextProvider, IConfiguration configuration, IUserContext userContext)
        {
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
                throw new CashSchedulerException("There is no such user", new[] { nameof(email) });
            }

            user.Password = password.Hash(Configuration);

            return UserRepository.Update(user);
        }

        public Task<User> Update(User user)
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
                targetUser.Balance = user.Balance;
            }

            return UserRepository.Update(targetUser);
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

            return UserRepository.Update(user);
        }
        
        public Task<User> Delete(int id)
        {
            return UserRepository.Delete(id);
        }
    }
}
