using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Exceptions;
using CashSchedulerWebServer.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CashSchedulerWebServer.Utils;

namespace CashSchedulerWebServer.Db.Repositories
{
    public class UserRepository : IUserRepository
    {
        private CashSchedulerContext Context { get; }

        public UserRepository(CashSchedulerContext context)
        {
            Context = context;
        }


        public IEnumerable<User> GetAll()
        {
            return Context.Users;
        }

        public User GetByKey(int id)
        {
            return Context.Users.FirstOrDefault(user => user.Id == id);
        }

        public User GetByEmail(string email)
        {
            return Context.Users.FirstOrDefault(user => user.Email == email);
        }

        public bool HasWithEmail(string email)
        {
            return Context.Users.Any(user => user.Email == email);
        }

        public async Task<User> Create(User user)
        {
            ModelValidator.ValidateModelAttributes(user);
            
            await Context.Users.AddAsync(user);
            await Context.SaveChangesAsync();
            
            return GetByEmail(user.Email);
        }

        public async Task<User> Update(User user)
        {
            ModelValidator.ValidateModelAttributes(user);
            
            Context.Users.Update(user);
            await Context.SaveChangesAsync();
            
            return user;
        }

        public async Task<User> Delete(int id)
        {
            var user = GetByKey(id);

            Context.Users.Remove(user);
            await Context.SaveChangesAsync();

            return user;
        }
    }
}
