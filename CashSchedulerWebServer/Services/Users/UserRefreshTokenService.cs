using System.Collections.Generic;
using System.Threading.Tasks;
using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Exceptions;
using CashSchedulerWebServer.Models;
using CashSchedulerWebServer.Services.Contracts;

namespace CashSchedulerWebServer.Services.Users
{
    public class UserRefreshTokenService : IUserRefreshTokenService
    {
        private IUserRefreshTokenRepository RefreshTokenRepository { get; }

        public UserRefreshTokenService(IContextProvider contextProvider)
        {
            RefreshTokenRepository = contextProvider.GetRepository<IUserRefreshTokenRepository>();
        }


        public Task<UserRefreshToken> Create(UserRefreshToken refreshToken)
        {
            return RefreshTokenRepository.Create(refreshToken);
        }

        public Task<UserRefreshToken> Update(UserRefreshToken refreshToken)
        {
            var token = RefreshTokenRepository.GetByKey(refreshToken.Id);
            if (token == null)
            {
                throw new CashSchedulerException("There is no such refresh token");
            }

            token.Token = refreshToken.Token;
            token.ExpiredDate = refreshToken.ExpiredDate;

            return RefreshTokenRepository.Update(token);
        }

        public Task<UserRefreshToken> Delete(int id)
        {
            var token = RefreshTokenRepository.GetByKey(id);
            if (token == null)
            {
                throw new CashSchedulerException("There is no such refresh token");
            }

            return RefreshTokenRepository.Delete(id);
        }

        public Task<IEnumerable<UserRefreshToken>> DeleteAllUserTokens(int userId)
        {
            return RefreshTokenRepository.Delete(RefreshTokenRepository.GetByUserId(userId));
        }

        public Task<IEnumerable<UserRefreshToken>> DeleteAllAppTokens(int userId)
        {
            return RefreshTokenRepository.Delete(RefreshTokenRepository.GetAppTokensByUserId(userId));
        }
    }
}
