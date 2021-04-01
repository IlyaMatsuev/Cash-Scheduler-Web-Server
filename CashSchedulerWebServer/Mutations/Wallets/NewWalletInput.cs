using HotChocolate;

namespace CashSchedulerWebServer.Mutations.Wallets
{
    public class NewWalletInput
    {
        [GraphQLNonNullType]
        public string Name { get; set; }

        public double Balance { get; set; }

        [GraphQLNonNullType]
        public string CurrencyAbbreviation { get; set; }

        public bool IsDefault { get; set; }
    }
}
