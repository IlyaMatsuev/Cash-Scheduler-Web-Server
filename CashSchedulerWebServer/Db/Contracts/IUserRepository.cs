using CashSchedulerWebServer.Models;
using System.Threading.Tasks;

namespace CashSchedulerWebServer.Db.Contracts
{
    public interface IUserRepository : IRepository<int, User>
    {
        User GetById();
        User GetUserByEmail(string email);
        bool HasUserWithEmail(string email);
        Task<User> UpdatePassword(string email, string password);
        Task<User> UpdateBalance(Transaction transaction, Transaction oldTransaction, bool isCreate = false, bool isUpdate = false, bool isDelete = false);
    }
}
