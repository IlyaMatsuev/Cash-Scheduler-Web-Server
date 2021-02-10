using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Exceptions;
using CashSchedulerWebServer.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CashSchedulerWebServer.Auth.Contracts;
using CashSchedulerWebServer.Utils;

namespace CashSchedulerWebServer.Db.Repositories
{
    public class UserRepository : IUserRepository
    {
        private CashSchedulerContext Context { get; }
        private int UserId { get; }

        public UserRepository(CashSchedulerContext context, IUserContext userContext)
        {
            Context = context;
            UserId = userContext.GetUserId();
        }


        public IEnumerable<User> GetAll()
        {
            return Context.Users;
        }

        public User GetById()
        {
            return GetByKey(UserId);
        }

        public User GetByKey(int id)
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

        public async Task<User> Create(User user)
        {
            ModelValidator.ValidateModelAttributes(user);
            
            await Context.Users.AddAsync(user);
            await Context.SaveChangesAsync();
            
            return GetUserByEmail(user.Email);
        }

        public async Task<User> Update(User user)
        {
            ModelValidator.ValidateModelAttributes(user);
            
            Context.Users.Update(user);
            await Context.SaveChangesAsync();
            
            return user;
        }

        public Task<User> Delete(int id)
        {
            throw new CashSchedulerException("It's forbidden to delete users");
        }
    }
}
