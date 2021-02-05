using System.Collections.Generic;
using CashSchedulerWebServer.Auth;
using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Models;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Types;

#nullable enable

namespace CashSchedulerWebServer.Queries.Wallets
{
    [ExtendObjectType(Name = "Query")]
    public class WalletQueries
    {
        [Authorize(Policy = AuthOptions.AUTH_POLICY)]
        public IEnumerable<Wallet>? Wallets([Service] IContextProvider contextProvider)
        {
            return contextProvider.GetRepository<IWalletRepository>().GetAll();
        }
    }
}
