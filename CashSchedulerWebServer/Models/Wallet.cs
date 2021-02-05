using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HotChocolate;

namespace CashSchedulerWebServer.Models
{
    public class Wallet
    {
        [Key]
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Wallet must have a name")]
        public string Name { get; set; }
        
        [DefaultValue(0)]
        public double Balance { get; set; }
        
        [Required(ErrorMessage = "Wallet must be related to a currency")]
        public Currency Currency { get; set; }
        
        [NotMapped]
        [GraphQLIgnore]
        public string CurrencyAbbreviation { get; set; }

        [Required(ErrorMessage = "Wallet must be related to a user")]
        public User User { get; set; }
        
        [DefaultValue(true)]
        public bool IsCustom { get; set; }
        
        [DefaultValue(false)]
        public bool IsDefault { get; set; }
    }
}
