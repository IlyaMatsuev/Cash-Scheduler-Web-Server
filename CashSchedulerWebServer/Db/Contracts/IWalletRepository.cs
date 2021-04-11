using System.Collections.Generic;
using System.Threading.Tasks;
using CashSchedulerWebServer.Models;

namespace CashSchedulerWebServer.Db.Contracts
{
    public interface IWalletRepository : IRepository<int, Wallet>
    {
        Wallet GetDefault();

        Task<IEnumerable<Wallet>> Update(IEnumerable<Wallet> wallets);

        IEnumerable<Wallet> DeleteByUserId(int userId);
    }
}
