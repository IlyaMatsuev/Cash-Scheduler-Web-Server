using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Exceptions;
using CashSchedulerWebServer.Models;
using CashSchedulerWebServer.Utils;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CashSchedulerWebServer.Auth;
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


        public UserRefreshToken GetByKey(int id)
        {
            return Context.UserRefreshTokens.FirstOrDefault(t => t.Id == id);
        }

        public IEnumerable<UserRefreshToken> GetByUserId(int id)
        {
            return Context.UserRefreshTokens
                .Where(t => t.User.Id == id && t.Type == (int) AuthOptions.TokenType.Refresh)
                .Include(t => t.User);
        }

        public IEnumerable<UserRefreshToken> GetAppTokensByUserId(int id)
        {
            return Context.UserRefreshTokens
                .Where(t => t.User.Id == id && t.Type == (int) AuthOptions.TokenType.AppRefresh)
                .Include(t => t.User);
        }

        public UserRefreshToken GetByUserAndToken(int userId, string token)
        {
            return Context.UserRefreshTokens
                .Where(t => t.User.Id == userId && t.Token == token)
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
            var token = GetByKey(id);

            Context.UserRefreshTokens.Remove(token);
            await Context.SaveChangesAsync();

            return token;
        }

        public async Task<IEnumerable<UserRefreshToken>> Delete(IEnumerable<UserRefreshToken> tokens)
        {
            Context.UserRefreshTokens.RemoveRange(tokens);
            await Context.SaveChangesAsync();

            return tokens;
        }

        public IEnumerable<UserRefreshToken> DeleteByUserId(int userId)
        {
            var tokens = Context.UserRefreshTokens.Where(c => c.User.Id == userId);

            Context.UserRefreshTokens.RemoveRange(tokens);
            Context.SaveChanges();

            return tokens;
        }
    }
}
