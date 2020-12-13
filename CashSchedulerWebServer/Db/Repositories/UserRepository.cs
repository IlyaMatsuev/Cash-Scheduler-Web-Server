using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Exceptions;
using CashSchedulerWebServer.Models;
using CashSchedulerWebServer.Utils;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CashSchedulerWebServer.Db.Repositories
{
    public class UserRepository : IUserRepository
    {
        private CashSchedulerContext Context { get; set; }
        private ClaimsPrincipal User { get; set; }
        private int? UserId => Convert.ToInt32(User?.Claims.FirstOrDefault(claim => claim.Type == "Id")?.Value ?? "-1");

        public UserRepository(CashSchedulerContext context, IHttpContextAccessor httpAccessor)
        {
            Context = context;
            User = httpAccessor.HttpContext.User;
        }


        public IEnumerable<User> GetAll()
        {
            return Context.Users;
        }

        public User GetById()
        {
            return GetById((int) UserId);
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

            user.Password = user.Password.Hash();
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

            user.Password = password.Hash();

            Context.Users.Update(user);
            await Context.SaveChangesAsync();

            return user;
        }

        public async Task<User> Update(User user)
        {
            ModelValidator.ValidateModelAttributes(user);
            Context.Users.Update(user);
            await Context.SaveChangesAsync();
            return user;
        }

        public async Task<User> UpdateBalance(Transaction transaction, Transaction oldTransaction, bool isCreate = false, bool isUpdate = false, bool isDelete = false)
        {
            // TODO: we don't want to use exact names from db records
            int delta = 1;
            User user = transaction.CreatedBy;

            if (transaction.TransactionCategory.Type.Name == "Expense")
            {
                delta = -1;
            }

            if (isCreate)
            {
                if (transaction.Date <= DateTime.UtcNow)
                {
                    user.Balance += transaction.Amount * delta;
                }
            }
            else if (isUpdate)
            {
                if (transaction.Date <= DateTime.UtcNow && oldTransaction.Date <= DateTime.UtcNow)
                {
                    user.Balance += (transaction.Amount - oldTransaction.Amount) * delta;
                }
                else if (transaction.Date <= DateTime.UtcNow && oldTransaction.Date > DateTime.UtcNow)
                {
                    user.Balance += transaction.Amount * delta;
                }
                else if (transaction.Date > DateTime.UtcNow && oldTransaction.Date <= DateTime.UtcNow)
                {
                    user.Balance -= oldTransaction.Amount * delta;
                }
            }
            else if (isDelete)
            {
                if (oldTransaction.Date <= DateTime.UtcNow)
                {
                    user.Balance -= oldTransaction.Amount * delta;
                }
            }

            return await Update(user);
        }

        public Task<User> Delete(int id) => throw new NotImplementedException();
    }
}
