﻿using System.Collections.Generic;
using System.Threading.Tasks;

namespace CashSchedulerWebServer.Db.Contracts
{
    public interface IRepository<in TKey, TModel>
    {
        IEnumerable<TModel> GetAll();
        // TODO: rename to GetByKey(TKey key)
        TModel GetById(TKey id);
        Task<TModel> Create(TModel entity);
        Task<TModel> Update(TModel entity);
        // TODO: rename to Delete(TKey key)
        Task<TModel> Delete(TKey entityId);
    }
}
