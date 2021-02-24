using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HotChocolate;

namespace CashSchedulerWebServer.Models
{
    public class UserSetting
    {
        [Key]
        public int Id { get; set; }

        [NotMapped]
        [GraphQLIgnore]
        public string Name { get; set; }

        [GraphQLNonNullType]
        public string Value { get; set; }

        [GraphQLNonNullType]
        [Required(ErrorMessage = "User is not linked")]
        public User User { get; set; }
        
        [GraphQLNonNullType]
        [Required(ErrorMessage = "Setting is not linked")]
        public Setting Setting { get; set; }
    }
}
