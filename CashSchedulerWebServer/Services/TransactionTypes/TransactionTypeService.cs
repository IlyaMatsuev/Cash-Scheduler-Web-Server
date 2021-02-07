using System.Collections.Generic;
using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Models;

namespace CashSchedulerWebServer.Services.TransactionTypes
{
    public class TransactionTypeService
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
    }
}
