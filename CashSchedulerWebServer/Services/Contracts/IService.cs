using System.Collections.Generic;
using System.Threading.Tasks;

namespace CashSchedulerWebServer.Services.Contracts
{
    public interface IService<in TKey, TModel>
    {
        IEnumerable<TModel> GetAll();
        TModel GetByKey(TKey id);
        Task<TModel> Create(TModel entity);
        Task<TModel> Update(TModel entity);
        Task<TModel> Delete(TKey entityId);
    }
}
