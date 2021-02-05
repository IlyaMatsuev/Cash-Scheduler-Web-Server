using CashSchedulerWebServer.Models;

namespace CashSchedulerWebServer.Db.Contracts
{
    public interface IUserRefreshTokenRepository : IRepository<int, UserRefreshToken>
    {
        UserRefreshToken GetByUserId(int id);
    }
}
