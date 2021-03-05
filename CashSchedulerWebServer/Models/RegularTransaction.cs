using CashSchedulerWebServer.Models.Validations;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HotChocolate;
using HotChocolate.Types;

namespace CashSchedulerWebServer.Models
{
    public class RegularTransaction
    {
        [NotMapped]
        private const double MAX_AMOUNT_VALUE = 100000000000;
        [NotMapped]
        private const double MIN_AMOUNT_VALUE = 0.01;

        
        [Key]
        public int Id { get; set; }
        
        [MaxLength(30, ErrorMessage = "Title cannot contain more than 30 characters")]
        public string Title { get; set; }
        
        [GraphQLNonNullType]
        public User User { get; set; }
        
        [GraphQLNonNullType]
        public Category Category { get; set; }
        
        [GraphQLNonNullType]
        public Wallet Wallet { get; set; }
        
        [NotMapped]
        [GraphQLIgnore]
        [Required(ErrorMessage = "Transaction must be linked to a wallet")]
        public int WalletId { get; set; }
        
        [NotMapped]
        [GraphQLIgnore]
        [Required(ErrorMessage = "You need to choose any category for the transaction")]
        public int CategoryId { get; set; }
        
        [Required(ErrorMessage = "Amount for transaction cannot be empty")]
        [Range(MIN_AMOUNT_VALUE, MAX_AMOUNT_VALUE, ErrorMessage = "You can specify amount in range from 0.01 to 100000000000")]
        public double Amount { get; set; }
        
        [GraphQLType(typeof(DateType))]
        [Required(ErrorMessage = "Transaction date cannot be empty")]
        public DateTime Date { get; set; }
        
        [GraphQLNonNullType]
        [GraphQLType(typeof(DateType))]
        [GreaterThanToday(ErrorMessage = "Next transaction date must be greater than today")]
        public DateTime NextTransactionDate { get; set; }
        
        [Required(ErrorMessage = "Regular transaction interval cannot be empty")]
        [RegularExpression(@"^(day)|(week)|(month)|(year)$", ErrorMessage = "Interval must be one of the following: day, week, month, year")]
        public string Interval { get; set; }

        
        public RegularTransaction()
        {
            Date = DateTime.Today;
        }

        public enum IntervalOptions
        {
            Day,
            Week,
            Month,
            Year
        }
    }
}
