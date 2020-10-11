using CashSchedulerWebServer.Db.Contracts;
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

        public async Task<User> Create(User newUser)
        {
            ModelValidator.ValidateModelAttributes(newUser);

            newUser.Password = newUser.Password.Hash();
            Context.Users.Add(newUser);
            await Context.SaveChangesAsync();
            return GetUserByEmail(newUser.Email);
        }

        public Task<User> Update(User entity) => throw new NotImplementedException();
        public Task<User> Delete(int entityId) => throw new NotImplementedException();
    }
}
