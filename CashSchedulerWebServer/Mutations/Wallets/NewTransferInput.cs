namespace CashSchedulerWebServer.Mutations.Wallets
{
    public class NewTransferInput
    {
        public int SourceWalletId { get; set; }

        public int TargetWalletId { get; set; }

        public double Amount { get; set; }

        public float ExchangeRate { get; set; }
    }
}
