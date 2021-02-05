using System.ComponentModel.DataAnnotations;

namespace CashSchedulerWebServer.Models
{
    public class TransactionType
    {
        [Key]
        public string Name { get; set; }
        
        public string IconUrl { get; set; }

        public enum Options
        {
            Income,
            Expense
        }
    }
}
