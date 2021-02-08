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
                .Include(w => w.User)
                .Include(w => w.Currency);
        }

        public Wallet GetById(int id)
        {
            return Context.Wallets.Where(w => w.Id == id && w.User.Id == UserId)
                .Include(w => w.User)
                .Include(w => w.Currency)
                .FirstOrDefault();
        }

        public async Task<Wallet> Create(Wallet wallet)
        {
            ModelValidator.ValidateModelAttributes(wallet);

            await Context.Wallets.AddAsync(wallet);
            await Context.SaveChangesAsync();

            return GetById(wallet.Id);
        }

        public async Task<Wallet> Update(Wallet wallet)
        {
            ModelValidator.ValidateModelAttributes(wallet);

            Context.Wallets.Update(wallet);
            await Context.SaveChangesAsync();

            return wallet;
        }

        public async Task<Wallet> Delete(int id)
        {
            var wallet = GetById(id);

            Context.Wallets.Remove(wallet);
            await Context.SaveChangesAsync();

            return wallet;
        }
    }
}
