using System.ComponentModel.DataAnnotations;
using CashSchedulerWebServer.Auth;
using HotChocolate;

namespace CashSchedulerWebServer.Models
{
    public class BugReport
    {
        [MaxLength(80, ErrorMessage = "Full name cannot contain more than 80 characters")]
        public string Name { get; set; }

        [GraphQLNonNullType]
        [Required(ErrorMessage = "Email is a required field")]
        [RegularExpression(AuthOptions.EMAIL_REGEX, ErrorMessage = "Your email address is in invalid format")]
        public string Email { get; set; }

        [MaxLength(40, ErrorMessage = "Phone cannot contain more than 40 characters")]
        [RegularExpression(AuthOptions.PHONE_REGEX, ErrorMessage = "Your phone is in invalid format")]
        public string Phone { get; set; }

        [GraphQLNonNullType]
        [Required(ErrorMessage = "Subject is a required field")]
        [MaxLength(80, ErrorMessage = "Subject cannot contain more than 80 characters")]
        public string Subject { get; set; }

        [GraphQLNonNullType]
        [Required(ErrorMessage = "Description is a required field")]
        public string Description { get; set; }
    }
}
