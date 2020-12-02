using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CashSchedulerWebServer.Models
{
    public class Transaction
    {
        [NotMapped]
        private const double MAX_AMOUNT_VALUE = 100000000000;
        [NotMapped]
        private const double MIN_AMOUNT_VALUE = 0.01;

        [Key]
        public int Id { get;set; }
        [MaxLength(30, ErrorMessage = "Title cannot contain more than 30 characters")]
        public string Title { get; set; }
        public User CreatedBy { get; set; }
        public Category TransactionCategory { get; set; }
        [Required(ErrorMessage = "You need to choose any category for the transaction")]
        [NotMapped]
        public int CategoryId { get; set; }
        [Required(ErrorMessage = "Amount for transaction cannot be empty"), Range(MIN_AMOUNT_VALUE, MAX_AMOUNT_VALUE, ErrorMessage = "You can specify amount in range from 0.01 to 100000000000")]
        public double Amount { get; set; }
        [Required(ErrorMessage = "Transaction date cannot be empty")]
        public DateTime Date { get; set; }

        public Transaction()
        {
            Date = DateTime.UtcNow;
        }
    }
}
