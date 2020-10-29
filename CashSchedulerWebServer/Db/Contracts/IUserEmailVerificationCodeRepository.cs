using CashSchedulerWebServer.Models;

namespace CashSchedulerWebServer.Db.Contracts
{
    interface IUserEmailVerificationCodeRepository : IRepository<UserEmailVerificationCode>
    {
        UserEmailVerificationCode GetByUserId(int id);
    }
}
