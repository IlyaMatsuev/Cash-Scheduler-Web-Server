using System;
using System.ComponentModel.DataAnnotations;

namespace CashSchedulerWebServer.Models
{
    public class UserEmailVerificationCode
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "The code is a required field")]
        public string Code { get; set; }
        
        [Required(ErrorMessage = "Expiration date is a require field")]
        public DateTime ExpiredDate { get; set; }
        
        [Required(ErrorMessage = "Code must be related to a user")]
        public User User { get; set; }

        public UserEmailVerificationCode() { }
        public UserEmailVerificationCode(string code, DateTime expiresIn, User user)
        {
            Code = code;
            ExpiredDate = expiresIn;
            User = user;
        }
    }
}
