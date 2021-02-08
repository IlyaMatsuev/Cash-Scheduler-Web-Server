using System.Threading.Tasks;
using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Exceptions;
using CashSchedulerWebServer.Models;
using CashSchedulerWebServer.Services.Contracts;

namespace CashSchedulerWebServer.Services.Users
{
    public class UserRefreshTokenService : IUserRefreshTokenService
    {
        private IContextProvider ContextProvider { get; }

        public UserRefreshTokenService(IContextProvider contextProvider)
        {
            ContextProvider = contextProvider;
        }
        

        public UserRefreshToken GetByUserId(int id)
        {
            return ContextProvider.GetRepository<IUserRefreshTokenRepository>().GetByUserId(id);
        }

        public Task<UserRefreshToken> Create(UserRefreshToken refreshToken)
        {
            return ContextProvider.GetRepository<IUserRefreshTokenRepository>().Create(refreshToken);
        }

        public Task<UserRefreshToken> Update(UserRefreshToken refreshToken)
        {
            var tokenRepository = ContextProvider.GetRepository<IUserRefreshTokenRepository>();
            
            var token = GetByUserId(refreshToken.User.Id);
            if (token == null)
            {
                return tokenRepository.Create(refreshToken);
            }

            token.Token = refreshToken.Token;
            token.ExpiredDate = refreshToken.ExpiredDate;

            return tokenRepository.Update(token);
        }

        public Task<UserRefreshToken> Delete(int id)
        {
            var tokenRepository = ContextProvider.GetRepository<IUserRefreshTokenRepository>();
            
            var token = tokenRepository.GetById(id);
            if (token == null)
            {
                throw new CashSchedulerException("There is no such refresh token");
            }
            
            return tokenRepository.Delete(id);
        }
    }
}
