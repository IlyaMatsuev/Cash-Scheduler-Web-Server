using System.Collections.Generic;
using System.Threading.Tasks;
using CashSchedulerWebServer.Models;

namespace CashSchedulerWebServer.Services.Contracts
{
    public interface IWalletService : IService<int, Wallet>
    {
        IEnumerable<Wallet> GetAll();

        Task<Wallet> CreateDefault(User user);

        Task<Wallet> Update(Wallet wallet, bool convertBalance, float? exchangeRate = null);

        Task<Wallet> UpdateBalance(
            Transaction transaction,
            Transaction oldTransaction,
            bool isCreate = false,
            bool isUpdate = false,
            bool isDelete = false);

        Task<IEnumerable<Wallet>> UpdateBalance(
            IEnumerable<Transaction> transactions,
            IEnumerable<Transaction> oldTransactions,
            bool isCreate = false,
            bool isUpdate = false,
            bool isDelete = false);

        Task<Transfer> CreateTransfer(Transfer transfer);
    }
}
