using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CashSchedulerWebServer.Auth;
using HotChocolate;

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

        [GraphQLNonNullType]
        [Required(ErrorMessage = "Email is a required field")]
        [RegularExpression(AuthOptions.EMAIL_REGEX, ErrorMessage = "Your email address is in invalid format")]
        public string Email { get; set; }

        [GraphQLIgnore]
        public string Password { get; set; }

        [NotMapped]
        [GraphQLIgnore]
        public double Balance { get; set; }
        
        [NotMapped]
        [GraphQLIgnore]
        public DateTime? LastLoginDate { get; set; }
    }
}
