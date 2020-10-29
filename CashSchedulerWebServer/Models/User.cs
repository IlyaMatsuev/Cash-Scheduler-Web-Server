using CashSchedulerWebServer.Authentication;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace CashSchedulerWebServer.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        [MaxLength(50, ErrorMessage = "First name cannot contain more than 50 characters")]
        public string FirstName { get; set; }
        [MaxLength(50, ErrorMessage = "Last name cannot contain more than 50 characters")]
        public string LastName { get; set; }
        [Required(ErrorMessage = "Email is a required field"), RegularExpression(AuthOptions.EMAIL_REGEX, ErrorMessage = "Your email address is in invalid format")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Password is a required field"),
            RegularExpression(AuthOptions.PASSWORD_REGEX, ErrorMessage = "Your password is too week. Consider to choose something with upper and lower case, digits and special characters with min and max length of 8 and 15")]
        public string Password { get; set; }
        [DefaultValue(0)]
        public double Balance { get; set; }
    }
}
