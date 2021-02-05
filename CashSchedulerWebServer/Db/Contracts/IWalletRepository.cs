using System.Threading.Tasks;
using CashSchedulerWebServer.Models;

namespace CashSchedulerWebServer.Db.Contracts
{
    public interface IWalletRepository : IRepository<int, Wallet>
    {
        Task<Wallet> CreateDefault(User user);
    }
}
