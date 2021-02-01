using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Exceptions;
using CashSchedulerWebServer.Models;
using CashSchedulerWebServer.Utils;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CashSchedulerWebServer.Auth.Contracts;

namespace CashSchedulerWebServer.Db.Repositories
{
    public class UserRepository : IUserRepository
    {
        private CashSchedulerContext Context { get; }
        private IConfiguration Configuration { get; }
        private int UserId { get; }

        public UserRepository(CashSchedulerContext context, IUserContext userContext, IConfiguration configuration)
        {
            Context = context;
            Configuration = configuration;
            UserId = userContext.GetUserId();
        }


        public IEnumerable<User> GetAll()
        {
            return Context.Users;
        }

        public User GetById()
        {
            return GetById(UserId);
        }

        public User GetById(int id)
        {
            return Context.Users.FirstOrDefault(user => user.Id == id);
        }

        public User GetUserByEmail(string email)
        {
            return Context.Users.FirstOrDefault(user => user.Email == email);
        }

        public bool HasUserWithEmail(string email)
        {
            return Context.Users.Any(user => user.Email == email);
        }

        public User GetUserByEmailAndPassword(string email, string passwordHash)
        {
            return Context.Users.FirstOrDefault(user => user.Email == email && user.Password == passwordHash);
        }

        public bool HasUserWithEmailAndPassword(string email, string passwordHash)
        {
            return Context.Users.Any(user => user.Email == email && user.Password == passwordHash);
        }

        public async Task<User> Create(User user)
        {
            ModelValidator.ValidateModelAttributes(user);

            user.Password = user.Password.Hash(Configuration);
            Context.Users.Add(user);
            await Context.SaveChangesAsync();
            return GetUserByEmail(user.Email);
        }

        public async Task<User> UpdatePassword(string email, string password)
        {
            var user = GetUserByEmail(email);
            if (user == null)
            {
                throw new CashSchedulerException("There is no such user", new[] { nameof(email) });
            }

            ModelValidator.ValidateModelAttributes(user);

            user.Password = password.Hash(Configuration);

            Context.Users.Update(user);
            await Context.SaveChangesAsync();

            return user;
        }

        public async Task<User> Update(User user)
        {
            var targetUser = GetById(user.Id);
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
            Context.Users.Update(targetUser);
            await Context.SaveChangesAsync();
            
            return targetUser;
        }

        public async Task<User> UpdateBalance(Transaction transaction, Transaction oldTransaction, bool isCreate = false, bool isUpdate = false, bool isDelete = false)
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

            return await Update(user);
        }

        public Task<User> Delete(int id)
        {
            throw new CashSchedulerException("It's forbidden to delete users` accounts");
        }
    }
}
