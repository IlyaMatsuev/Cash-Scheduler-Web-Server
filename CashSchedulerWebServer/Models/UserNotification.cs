using System;
using System.ComponentModel.DataAnnotations;

namespace CashSchedulerWebServer.Models
{
    public class UserNotification
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Your notification must contain title")]
        [MaxLength(50, ErrorMessage = "Notification title can contain up to 50 characters")]
        public string Title { get; set; }

        [Required(ErrorMessage = "You cannot send notification without any content")]
        public string Content { get; set; }

        public User User { get; set; }

        [Required(ErrorMessage = "You need to specify if the notification is read or not")]
        public bool IsRead { get; set; }

        public DateTime CreatedDate { get; set; }


        public UserNotification()
        {
            IsRead = false;
            CreatedDate = DateTime.Today;
        }
    }
}
