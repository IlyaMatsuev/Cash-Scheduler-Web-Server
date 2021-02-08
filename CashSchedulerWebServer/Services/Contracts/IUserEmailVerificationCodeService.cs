using CashSchedulerWebServer.Models;

namespace CashSchedulerWebServer.Services.Contracts
{
    public interface IUserEmailVerificationCodeService : IService<int, UserEmailVerificationCode>
    {
        UserEmailVerificationCode GetByUserId(int id);
    }
}
