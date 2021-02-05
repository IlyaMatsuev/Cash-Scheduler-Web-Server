using System.Collections.Generic;
using System.Threading.Tasks;

namespace CashSchedulerWebServer.Db.Contracts
{
    public interface IRepository<in TKey, TModel>
    {
        IEnumerable<TModel> GetAll();
        TModel GetById(TKey id);
        Task<TModel> Create(TModel entity);
        Task<TModel> Update(TModel entity);
        Task<TModel> Delete(TKey entityId);
    }
}
