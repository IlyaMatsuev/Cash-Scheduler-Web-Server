using System.ComponentModel.DataAnnotations;

namespace CashSchedulerWebServer.Models
{
    public class Setting
    {
        [Key]
        public string Name { get; set; }

        [Required(ErrorMessage = "Setting must contain a label")]
        public string Label { get; set; }

        [Required(ErrorMessage = "Setting must contain a unit name")]
        public string UnitName { get; set; }

        [Required(ErrorMessage = "Setting must contain a section name")]
        public string SectionName { get; set; }

        [Required(ErrorMessage = "Type of the value field is not specified")]
        public string ValueType { get; set; }

        public string Description { get; set; }


        public enum UnitOptions
        {
            General,
            Notifications,
            Integrations
        }

        public enum SectionOptions
        {
            Balance,
            Appearance,
            Dangerous,
            Management,
            Sound,
            AppToken
        }

        public enum SettingOptions
        {
            ShowBalance,
            DarkTheme,
            Language,
            DeleteAccount,
            DuplicateToEmail,
            TurnNotificationsOn,
            TurnNotificationsSoundOn,
            ConnectedAppsToken
        }

        public enum ValueTypes
        {
            Checkbox,
            Text,
            Custom
        }
    }
}
