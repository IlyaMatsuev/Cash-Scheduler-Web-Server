using CashSchedulerWebServer.Models;
using System.Threading.Tasks;

namespace CashSchedulerWebServer.Db.Contracts
{
    interface IUserRepository : IRepository<User>
    {
        User GetById();
        User GetUserByEmail(string email);
        bool HasUserWithEmail(string email);
        User GetUserByEmailAndPassword(string email, string passwordHash);
        bool HasUserWithEmailAndPassword(string email, string passwordHash);
        Task<User> UpdatePassword(string email, string password);
        Task<User> UpdateBalance(Transaction transaction, Transaction oldTransaction, bool isCreate = false, bool isUpdate = false, bool isDelete = false);
    }
}
