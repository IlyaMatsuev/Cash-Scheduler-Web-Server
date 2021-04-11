using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CashSchedulerWebServer.Auth.Contracts;
using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Models;
using CashSchedulerWebServer.Utils;
using Microsoft.EntityFrameworkCore;

namespace CashSchedulerWebServer.Db.Repositories
{
    public class WalletRepository : IWalletRepository 
    {
        private CashSchedulerContext Context { get; }
        private int UserId { get; }

        public WalletRepository(CashSchedulerContext context, IUserContext userContext)
        {
            Context = context;
            UserId = userContext.GetUserId();
        }
        
        
        public IEnumerable<Wallet> GetAll()
        {
            return Context.Wallets.Where(w => w.User.Id == UserId)
                .OrderBy(w => !w.IsDefault)
                .Include(w => w.User)
                .Include(w => w.Currency);
        }
        
        public Wallet GetDefault()
        {
            return Context.Wallets.Where(w => w.User.Id == UserId && w.IsDefault)
                .Include(w => w.User)
                .Include(w => w.Currency)
                .FirstOrDefault();
        }

        public Wallet GetByKey(int id)
        {
            return Context.Wallets.Where(w => w.Id == id)
                .Include(w => w.User)
                .Include(w => w.Currency)
                .FirstOrDefault();
        }

        public async Task<Wallet> Create(Wallet wallet)
        {
            ModelValidator.ValidateModelAttributes(wallet);

            await Context.Wallets.AddAsync(wallet);
            await Context.SaveChangesAsync();

            return GetByKey(wallet.Id);
        }

        public async Task<Wallet> Update(Wallet wallet)
        {
            ModelValidator.ValidateModelAttributes(wallet);

            Context.Wallets.Update(wallet);
            await Context.SaveChangesAsync();

            return wallet;
        }

        public async Task<IEnumerable<Wallet>> Update(IEnumerable<Wallet> wallets)
        {
            Context.Wallets.UpdateRange(wallets);
            await Context.SaveChangesAsync();

            return wallets;
        }

        public async Task<Wallet> Delete(int id)
        {
            var wallet = GetByKey(id);

            var relatedTransactions = Context.Transactions.Where(t => t.Wallet.Id == id);
            var relatedRegularTransactions = Context.RegularTransactions.Where(t => t.Wallet.Id == id);

            Context.Transactions.RemoveRange(relatedTransactions);
            Context.RegularTransactions.RemoveRange(relatedRegularTransactions);
            Context.Wallets.Remove(wallet);
            await Context.SaveChangesAsync();

            return wallet;
        }

        public IEnumerable<Wallet> DeleteByUserId(int userId)
        {
            var wallets = Context.Wallets.Where(c => c.User.Id == userId);

            Context.Wallets.RemoveRange(wallets);
            Context.SaveChanges();

            return wallets;
        }
    }
}
