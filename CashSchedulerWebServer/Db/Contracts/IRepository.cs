using System.Collections.Generic;
using System.Threading.Tasks;

namespace CashSchedulerWebServer.Db.Contracts
{
    interface IRepository<T>
    {
        IEnumerable<T> GetAll();
        T GetById(int id);
        Task<T> Create(T entity);
        Task<T> Update(T entity);
        Task<T> Delete(int entityId);
    }
}
