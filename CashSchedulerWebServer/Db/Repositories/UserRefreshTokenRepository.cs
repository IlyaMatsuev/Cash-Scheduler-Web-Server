using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Exceptions;
using CashSchedulerWebServer.Models;
using CashSchedulerWebServer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CashSchedulerWebServer.Db.Repositories
{
    public class UserRefreshTokenRepository : IUserRefreshTokenRepository
    {
        private CashSchedulerContext Context { get; set; }

        public UserRefreshTokenRepository(CashSchedulerContext context)
        {
            Context = context;
        }


        public IEnumerable<UserRefreshToken> GetAll()
        {
            throw new CashSchedulerException("It's forbidden to fetch all the refresh tokens");
        }

        public UserRefreshToken GetById(int id)
        {
            return Context.UserRefreshTokens.FirstOrDefault(t => t.Id == id);
        }

        public UserRefreshToken GetByUserId(int id)
        {
            return Context.UserRefreshTokens.FirstOrDefault(t => t.User.Id == id);
        }

        public async Task<UserRefreshToken> Create(UserRefreshToken refreshToken)
        {
            ModelValidator.ValidateModelAttributes(refreshToken);
            Context.UserRefreshTokens.Add(refreshToken);
            await Context.SaveChangesAsync();
            return refreshToken;
        }

        public async Task<UserRefreshToken> Update(UserRefreshToken refreshToken)
        {
            var token = GetByUserId(refreshToken.User.Id);
            if (token == null)
            {
                ModelValidator.ValidateModelAttributes(refreshToken);
                Context.UserRefreshTokens.Add(token = refreshToken);
            }
            else
            {
                token.Token = refreshToken.Token;
                token.ExpiredDate = refreshToken.ExpiredDate;
                ModelValidator.ValidateModelAttributes(token);

                Context.UserRefreshTokens.Update(token);
            }

            await Context.SaveChangesAsync();

            return token;
        }

        public async Task<UserRefreshToken> Delete(int tokenId)
        {
            var token = GetById(tokenId);
            if (token == null)
            {
                throw new CashSchedulerException("There is no such refresh token");
            }
            Context.UserRefreshTokens.Remove(token);
            await Context.SaveChangesAsync();
            return token;
        }
    }
}
