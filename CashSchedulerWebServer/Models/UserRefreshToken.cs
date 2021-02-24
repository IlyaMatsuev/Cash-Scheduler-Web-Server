using System;
using System.ComponentModel.DataAnnotations;

namespace CashSchedulerWebServer.Models
{
    public class UserRefreshToken
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "The string token is required for the resresh token")]
        public string Token { get; set; }
        
        [Required(ErrorMessage = "Expiration date is required for the refresh token")]
        public DateTime ExpiredDate { get; set; }
        
        [Required(ErrorMessage = "Token must be related to a user")]
        public User User { get; set; }
    }
}
