using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HotChocolate;

namespace CashSchedulerWebServer.Models
{
    public class Category
    {
        [NotMapped]
        private const string DEFAULT_ICON_URL = "/static/icons/categories/unknown.png";

        
        [Key]
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Category name cannot be empty")]
        [StringLength(30, MinimumLength = 2, ErrorMessage = "Category name must have at least 2 and no more than 30 characters")]
        public string Name { get; set; }
        
        [GraphQLNonNullType]
        public TransactionType Type { get; set; }
        
        [NotMapped]
        [GraphQLIgnore]
        [Required(ErrorMessage = "Transaction type name cannot be empty")]
        public string TypeName { get; set; }
        
        public User User { get; set; }

        [DefaultValue(true)]
        public bool IsCustom { get; set; }

        [Required(ErrorMessage = "Category Icon cannot be empty")]
        public string IconUrl
        {
            get => iconUrl;
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    iconUrl = value;
                }
            }
        }
        
        [NotMapped]
        private string iconUrl = DEFAULT_ICON_URL;
    }
}
