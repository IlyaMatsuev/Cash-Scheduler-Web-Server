using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Exceptions;
using CashSchedulerWebServer.Models;
using CashSchedulerWebServer.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace CashSchedulerWebServer.Db.Repositories
{
    public class UserRefreshTokenRepository : IUserRefreshTokenRepository
    {
        private CashSchedulerContext Context { get; }

        public UserRefreshTokenRepository(CashSchedulerContext context)
        {
            Context = context;
        }


        public UserRefreshToken GetById(int id)
        {
            return Context.UserRefreshTokens.FirstOrDefault(t => t.Id == id);
        }

        public UserRefreshToken GetByUserId(int id)
        {
            return Context.UserRefreshTokens
                .Where(t => t.User.Id == id)
                .Include(t => t.User)
                .FirstOrDefault();
        }
        
        public IEnumerable<UserRefreshToken> GetAll()
        {
            throw new CashSchedulerException("It's forbidden to fetch all the refresh tokens");
        }

        public async Task<UserRefreshToken> Create(UserRefreshToken refreshToken)
        {
            ModelValidator.ValidateModelAttributes(refreshToken);
            
            await Context.UserRefreshTokens.AddAsync(refreshToken);
            await Context.SaveChangesAsync();
            
            return refreshToken;
        }

        public async Task<UserRefreshToken> Update(UserRefreshToken refreshToken)
        {
            ModelValidator.ValidateModelAttributes(refreshToken);
            
            Context.UserRefreshTokens.Update(refreshToken);
            await Context.SaveChangesAsync();
            
            return refreshToken;
        }

        public async Task<UserRefreshToken> Delete(int id)
        {
            var token = GetById(id);
            
            Context.UserRefreshTokens.Remove(token);
            await Context.SaveChangesAsync();
            
            return token;
        }
    }
}
