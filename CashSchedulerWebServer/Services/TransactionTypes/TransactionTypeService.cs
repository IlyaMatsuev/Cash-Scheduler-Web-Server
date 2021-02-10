using System.Collections.Generic;
using System.Threading.Tasks;
using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Models;
using CashSchedulerWebServer.Services.Contracts;

namespace CashSchedulerWebServer.Services.TransactionTypes
{
    public class TransactionTypeService : ITransactionTypeService
    {
        private ITransactionTypeRepository TransactionTypeRepository { get; }
        
        public TransactionTypeService(IContextProvider contextProvider)
        {
            TransactionTypeRepository = contextProvider.GetRepository<ITransactionTypeRepository>();
        }
        
        
        public IEnumerable<TransactionType> GetAll()
        {
            return TransactionTypeRepository.GetAll();
        }

        public TransactionType GetByKey(string name)
        {
            return TransactionTypeRepository.GetByKey(name);
        }

        public Task<TransactionType> Create(TransactionType transactionType)
        {
            return TransactionTypeRepository.Create(transactionType);
        }

        public Task<TransactionType> Update(TransactionType transactionType)
        {
            return TransactionTypeRepository.Update(transactionType);
        }

        public Task<TransactionType> Delete(string name)
        {
            return TransactionTypeRepository.Delete(name);
        }
    }
}
