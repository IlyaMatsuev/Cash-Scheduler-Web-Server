using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HotChocolate;

namespace CashSchedulerWebServer.Models
{
    public class Wallet
    {
        [NotMapped]
        private const double MAX_AMOUNT_VALUE = 100000000000;
        [NotMapped]
        private const double MIN_AMOUNT_VALUE = 0;


        [Key]
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Wallet must have a name")]
        public string Name { get; set; }
        
        [Required(ErrorMessage = "Balance cannot be empty")]
        [Range(MIN_AMOUNT_VALUE, MAX_AMOUNT_VALUE, ErrorMessage = "You can specify balance in range from 0 to 100000000000")]
        public double Balance { get; set; }
        
        [Required(ErrorMessage = "Wallet must be related to a currency")]
        public Currency Currency { get; set; }
        
        [NotMapped]
        [GraphQLIgnore]
        public string CurrencyAbbreviation { get; set; }

        [Required(ErrorMessage = "Wallet must be related to a user")]
        public User User { get; set; }

        [DefaultValue(false)]
        public bool IsDefault { get; set; }
    }
}
