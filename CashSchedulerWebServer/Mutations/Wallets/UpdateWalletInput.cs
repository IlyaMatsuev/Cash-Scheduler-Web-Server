namespace CashSchedulerWebServer.Mutations.Wallets
{
    public class UpdateWalletInput
    {
        public int Id { get; set; }
        
        public string Name { get; set; }
        
        public double? Balance { get; set; }
        
        public string CurrencyAbbreviation { get; set; }
    }
}
