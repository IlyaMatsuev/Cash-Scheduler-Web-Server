using System.ComponentModel.DataAnnotations;
using HotChocolate;

namespace CashSchedulerWebServer.Models
{
    public class UserSetting
    {
        [Key]
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Setting must contain a name")]
        public string Name { get; set; }
        
        public string Value { get; set; }
        
        [Required(ErrorMessage = "Setting must contain a unit name")]
        public string UnitName { get; set; }
        
        public User User { get; set; }


        public enum SettingOptions
        {
            ShowBalance,
            TurnNotificationsOn,
            DuplicateToEmail,
            TurnNotificationsSoundOn
        }

        public enum UnitOptions
        {
            General,
            Notifications
        }
    }
}
