using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CashSchedulerWebServer.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        [Required(ErrorMessage = "Category name cannot be empty"), StringLength(30, MinimumLength = 2, ErrorMessage = "Category name must have at least 2 and no more than 30 characters")]
        public string Name { get; set; }
        public TransactionType Type { get; set; }
        [NotMapped]
        [Required(ErrorMessage = "Transaction type name cannot be empty")]
        public string TransactionTypeName { get; set; }
        public User CreatedBy { get; set; }
        [DefaultValue(false)]
        public bool IsCustom { get; set; }
        public string IconUrl { get; set; }
    }
}
