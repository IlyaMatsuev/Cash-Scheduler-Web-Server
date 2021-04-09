using CashSchedulerWebServer.Models;
using Microsoft.EntityFrameworkCore;

namespace CashSchedulerWebServer.Db
{
    public class CashSchedulerContext : DbContext
    {
        public CashSchedulerContext(DbContextOptions<CashSchedulerContext> options) : base(options)
        {
        }


        public DbSet<User> Users { get; set; }

        public DbSet<UserRefreshToken> UserRefreshTokens { get; set; }

        public DbSet<UserEmailVerificationCode> UserEmailVerificationCodes { get; set; }

        public DbSet<UserNotification> UserNotifications { get; set; }

        public DbSet<UserSetting> UserSettings { get; set; }

        public DbSet<Setting> Settings { get; set; }

        public DbSet<Language> Languages { get; set; }

        public DbSet<TransactionType> TransactionTypes { get; set; }

        public DbSet<Category> Categories { get; set; }

        public DbSet<Transaction> Transactions { get; set; }

        public DbSet<RegularTransaction> RegularTransactions { get; set; }

        public DbSet<Wallet> Wallets { get; set; }

        public DbSet<Currency> Currencies { get; set; }

        public DbSet<CurrencyExchangeRate> CurrencyExchangeRates { get; set; }
    }
}
