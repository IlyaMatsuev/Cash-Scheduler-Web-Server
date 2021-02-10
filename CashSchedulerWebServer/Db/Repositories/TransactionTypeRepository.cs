using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Exceptions;
using CashSchedulerWebServer.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CashSchedulerWebServer.Db.Repositories
{
    public class TransactionTypeRepository : ITransactionTypeRepository
    {
        private CashSchedulerContext Context { get; }

        public TransactionTypeRepository(CashSchedulerContext context)
        {
            Context = context;
        }


        public IEnumerable<TransactionType> GetAll()
        {
            return Context.TransactionTypes;
        }

        public TransactionType GetByKey(string name)
        {
            return Context.TransactionTypes.FirstOrDefault(t => t.Name == name);
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
