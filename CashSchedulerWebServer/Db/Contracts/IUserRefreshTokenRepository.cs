using CashSchedulerWebServer.Models;

namespace CashSchedulerWebServer.Db.Contracts
{
    interface IUserRefreshTokenRepository : IRepository<UserRefreshToken>
    {
        UserRefreshToken GetByUserId(int id);
    }
}
