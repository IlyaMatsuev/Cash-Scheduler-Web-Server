using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HotChocolate;

namespace CashSchedulerWebServer.Models
{
    public class CurrencyExchangeRate
    {
        [Key]
        public int Id { get; set; }
        
        [ForeignKey("SourceCurrencyId")]
        public Currency SourceCurrency { get; set; }
        
        [NotMapped]
        [GraphQLIgnore]
        public string SourceCurrencyAbbreviation { get; set; }
        
        [ForeignKey("TargetCurrencyId")]
        public Currency TargetCurrency { get; set; }
        
        [NotMapped]
        [GraphQLIgnore]
        public string TargetCurrencyAbbreviation { get; set; }
        
        [Required(ErrorMessage = "Exchange rate is a required field")]
        public float ExchangeRate { get; set; }

        public DateTime ValidFrom { get; set; } = DateTime.Today;

        public DateTime ValidTo { get; set; } = DateTime.Today;
        
        public User User { get; set; }
        
        [DefaultValue(true)]
        public bool IsCustom { get; set; }
    }
}
