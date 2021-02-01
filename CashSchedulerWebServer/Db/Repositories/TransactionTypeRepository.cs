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

        public TransactionType GetById(int id)
        {
            throw new CashSchedulerException("Transaction Types don't have an Id field, use GetByName() instead");
        }

        public TransactionType GetByName(string name)
        {
            return Context.TransactionTypes.FirstOrDefault(t => t.Name == name);
        }

        public async Task<TransactionType> Create(TransactionType transactionType)
        {
            Context.TransactionTypes.Add(transactionType);
            await Context.SaveChangesAsync();

            return GetByName(transactionType.Name);
        }

        public Task<TransactionType> Update(TransactionType transactionType)
        {
            throw new CashSchedulerException("It's forbidden to update the existing transaction types");
        }

        public Task<TransactionType> Delete(int transactionTypeId)
        {
            throw new CashSchedulerException("It's forbidden to delete existing transaction types");
        }
    }
}
