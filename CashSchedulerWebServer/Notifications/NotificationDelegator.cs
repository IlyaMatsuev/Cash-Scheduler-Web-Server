using System.Collections.Generic;
using System.IO;

namespace CashSchedulerWebServer.Notifications
{
    public class NotificationDelegator
    {
        private string NotificationTemplatesFolder { get; } = Directory.GetCurrentDirectory() + "/Content/EmailTemplates";

        private Dictionary<NotificationTemplateType, (string, string)> TemplatesMap { get; } =
            new Dictionary<NotificationTemplateType, (string, string)>
            {
                {NotificationTemplateType.VerificationCode, ("Verify It's You", "VerificationCode.html")},
                {
                    NotificationTemplateType.MostSpentCategoryForWeek,
                    ("Your most expensive category for the last week", "WeeklyCategoryReport.html")
                },
            };


        public NotificationTemplate GetTemplate(NotificationTemplateType templateType, Dictionary<string, string> parameters)
        {
            (string subject, string fileName) = TemplatesMap[templateType];
            return new NotificationTemplate(subject, File.ReadAllText($"{NotificationTemplatesFolder}/{fileName}"), parameters);
        }
    }
}
