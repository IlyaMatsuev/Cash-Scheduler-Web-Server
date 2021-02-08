using System.Collections.Generic;
using System.Threading.Tasks;
using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Models;
using CashSchedulerWebServer.Services.Contracts;

namespace CashSchedulerWebServer.Services.TransactionTypes
{
    public class TransactionTypeService : ITransactionTypeService
    {
        private IContextProvider ContextProvider { get; }
        
        public TransactionTypeService(IContextProvider contextProvider)
        {
            ContextProvider = contextProvider;
        }
        
        
        public IEnumerable<TransactionType> GetAll()
        {
            return ContextProvider.GetRepository<ITransactionTypeRepository>().GetAll();
        }

        public TransactionType GetByKey(string key)
        {
            return ContextProvider.GetRepository<ITransactionTypeRepository>().GetById(key);
        }

        public Task<TransactionType> Create(TransactionType transactionType)
        {
            return ContextProvider.GetRepository<ITransactionTypeRepository>().Create(transactionType);
        }

        public Task<TransactionType> Update(TransactionType transactionType)
        {
            return ContextProvider.GetRepository<ITransactionTypeRepository>().Update(transactionType);
        }

        public Task<TransactionType> Delete(string id)
        {
            return ContextProvider.GetRepository<ITransactionTypeRepository>().Delete(id);
        }
    }
}
