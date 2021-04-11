using System.Collections.Generic;
using System.Threading.Tasks;
using CashSchedulerWebServer.Models;

namespace CashSchedulerWebServer.Db.Contracts
{
    public interface IUserEmailVerificationCodeRepository : IRepository<int, UserEmailVerificationCode>
    {
        UserEmailVerificationCode GetByUserId(int id);

        IEnumerable<UserEmailVerificationCode> DeleteByUserId(int userId);
    }
}
