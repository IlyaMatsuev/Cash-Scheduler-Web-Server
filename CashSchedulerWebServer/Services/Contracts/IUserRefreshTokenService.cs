using CashSchedulerWebServer.Models;

namespace CashSchedulerWebServer.Services.Contracts
{
    public interface IUserRefreshTokenService : IService<int, UserRefreshToken>
    {
        UserRefreshToken GetByUserId(int id);
    }
}
