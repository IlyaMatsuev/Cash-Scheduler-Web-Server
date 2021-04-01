using System.Collections.Generic;
using System.Threading.Tasks;
using CashSchedulerWebServer.Models;

namespace CashSchedulerWebServer.Services.Contracts
{
    public interface IUserRefreshTokenService : IService<int, UserRefreshToken>
    {
        Task<IEnumerable<UserRefreshToken>> DeleteAllUserTokens(int userId);

        Task<IEnumerable<UserRefreshToken>> DeleteAllAppTokens(int userId);
    }
}
