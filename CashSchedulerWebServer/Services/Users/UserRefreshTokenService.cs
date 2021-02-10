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
        

        public UserRefreshToken GetByUserId(int id)
        {
            return RefreshTokenRepository.GetByUserId(id);
        }

        public Task<UserRefreshToken> Create(UserRefreshToken refreshToken)
        {
            return RefreshTokenRepository.Create(refreshToken);
        }

        public Task<UserRefreshToken> Update(UserRefreshToken refreshToken)
        {
            var token = GetByUserId(refreshToken.User.Id);
            if (token == null)
            {
                return RefreshTokenRepository.Create(refreshToken);
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
    }
}
