using CashSchedulerWebServer.Models;

namespace CashSchedulerWebServer.Db.Contracts
{
    public interface IUserRepository : IRepository<int, User>
    {
        User GetByEmail(string email);
        bool HasWithEmail(string email);
    }
}
