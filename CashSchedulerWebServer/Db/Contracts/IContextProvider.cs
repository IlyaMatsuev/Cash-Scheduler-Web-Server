using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;

namespace CashSchedulerWebServer.Db.Contracts
{
    public interface IContextProvider
    {
        T GetRepository<T>() where T : class;

        T GetService<T>() where T : class;

        Task<IDbContextTransaction> BeginTransaction();

        Task PreventTransaction(IDbContextTransaction transaction);

        Task EndTransaction(IDbContextTransaction transaction);
    }
}
