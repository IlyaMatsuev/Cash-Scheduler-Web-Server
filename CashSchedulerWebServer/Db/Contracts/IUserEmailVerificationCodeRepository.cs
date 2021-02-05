using CashSchedulerWebServer.Models;

namespace CashSchedulerWebServer.Db.Contracts
{
    public interface IUserEmailVerificationCodeRepository : IRepository<int, UserEmailVerificationCode>
    {
        UserEmailVerificationCode GetByUserId(int id);
    }
}
