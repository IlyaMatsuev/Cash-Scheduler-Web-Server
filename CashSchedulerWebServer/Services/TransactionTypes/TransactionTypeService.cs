using System.Collections.Generic;
using System.Threading.Tasks;
using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Exceptions;
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
            throw new CashSchedulerException("It's forbidden to create new transaction types");
        }

        public Task<TransactionType> Update(TransactionType transactionType)
        {
            throw new CashSchedulerException("It's forbidden to update the existing transaction types");
        }

        public Task<TransactionType> Delete(string name)
        {
            throw new CashSchedulerException("It's forbidden to delete the existing transaction types");
        }
    }
}
