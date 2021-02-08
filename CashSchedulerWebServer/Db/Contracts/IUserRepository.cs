using CashSchedulerWebServer.Models;

namespace CashSchedulerWebServer.Db.Contracts
{
    public interface IUserRepository : IRepository<int, User>
    {
        User GetById();
        User GetUserByEmail(string email);
        bool HasUserWithEmail(string email);
    }
}
