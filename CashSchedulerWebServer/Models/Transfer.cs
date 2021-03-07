using System.ComponentModel.DataAnnotations;

namespace CashSchedulerWebServer.Models
{
    public class Transfer
    {
        private const double MAX_AMOUNT_VALUE = 100000000000;
        private const double MIN_AMOUNT_VALUE = 0.01;
        
        
        [Required(ErrorMessage = "Source wallet is a required field")]
        public Wallet SourceWallet { get; set; }
        
        public int SourceWalletId { get; set; }
        
        [Required(ErrorMessage = "Target wallet is a required field")]
        public Wallet TargetWallet { get; set; }
        
        public int TargetWalletId { get; set; }
        
        [Required(ErrorMessage = "Amount is a required field")]
        [Range(MIN_AMOUNT_VALUE, MAX_AMOUNT_VALUE, ErrorMessage = "You can specify transfer amount in range from 0.01 to 100000000000")]
        public double Amount { get; set; }
        
        [Required(ErrorMessage = "Exchange rate is a required field")]
        public float ExchangeRate { get; set; }
    }
}
