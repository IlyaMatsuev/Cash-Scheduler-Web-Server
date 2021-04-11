using System.Collections.Generic;
using System.Threading.Tasks;
using CashSchedulerWebServer.Models;

namespace CashSchedulerWebServer.Db.Contracts
{
    public interface IUserRefreshTokenRepository : IRepository<int, UserRefreshToken>
    {
        IEnumerable<UserRefreshToken> GetByUserId(int userId);

        IEnumerable<UserRefreshToken> GetAppTokensByUserId(int userId);

        UserRefreshToken GetByUserAndToken(int userId, string token);

        Task<IEnumerable<UserRefreshToken>> Delete(IEnumerable<UserRefreshToken> tokens);

        IEnumerable<UserRefreshToken> DeleteByUserId(int userId);
    }
}
