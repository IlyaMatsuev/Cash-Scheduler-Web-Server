using System.Threading.Tasks;
using CashSchedulerWebServer.Models;

namespace CashSchedulerWebServer.Services.Contracts
{
    public interface IUserService : IService<int, User>
    {
        User GetById();

        User GetByEmail(string email);

        bool HasWithEmail(string email);

        Task<User> UpdatePassword(string email, string password);

        Task<User> Delete(string password);
    }
}
