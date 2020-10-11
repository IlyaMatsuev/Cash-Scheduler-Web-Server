using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Models;
using GraphQL;
using Microsoft.EntityFrameworkCore;
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
            throw new ExecutionError("It's forbidden to fetch all the refresh tokens");
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
            Context.UserRefreshTokens.Add(refreshToken);
            await Context.SaveChangesAsync();
            return refreshToken;
        }

        public async Task<UserRefreshToken> Update(UserRefreshToken refreshToken)
        {
            var token = Context.UserRefreshTokens.FirstOrDefault(t => t.User.Id == refreshToken.User.Id);
            if (token == null)
            {
                Context.UserRefreshTokens.Add(token = refreshToken);
            }
            else
            {
                token.Token = refreshToken.Token;
                token.ExpiredDate = refreshToken.ExpiredDate;
                Context.UserRefreshTokens.Update(token);
            }
            try
            {
                await Context.SaveChangesAsync();
            }
            catch (Exception error)
            {
                Console.WriteLine("Error: " + error.Message);
            }

            return token;
        }

        public async Task<UserRefreshToken> Delete(int tokenId)
        {
            var token = GetById(tokenId);
            Context.UserRefreshTokens.Remove(token);
            await Context.SaveChangesAsync();
            return token;
        }
    }
}
