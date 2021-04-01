using System;
using System.ComponentModel.DataAnnotations;
using CashSchedulerWebServer.Auth;

namespace CashSchedulerWebServer.Models
{
    public class UserRefreshToken
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "The string token is required for the refresh token")]
        public string Token { get; set; }

        [Required(ErrorMessage = "Expiration date is required for the refresh token")]
        public DateTime ExpiredDate { get; set; }

        [Required(ErrorMessage = "Token type is required for the refresh token")]
        public int Type { get; set; }

        [Required(ErrorMessage = "Token must be related to a user")]
        public User User { get; set; }

        public UserRefreshToken()
        {
            Type = (int) AuthOptions.TokenType.Refresh;
        }
    }
}
