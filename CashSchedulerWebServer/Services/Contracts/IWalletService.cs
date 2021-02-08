using System.Collections.Generic;
using System.Threading.Tasks;
using CashSchedulerWebServer.Models;

namespace CashSchedulerWebServer.Services.Contracts
{
    public interface IWalletService : IService<int, Wallet>
    {
        IEnumerable<Wallet> GetAll();
        Task<Wallet> CreateDefault(User user);
    }
}
