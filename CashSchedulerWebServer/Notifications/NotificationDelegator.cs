using System.Collections.Generic;
using System.IO;

namespace CashSchedulerWebServer.Notifications
{
    public class NotificationDelegator
    {
        private readonly string NOTIFICATION_TEMPLATES_FOLDER = Directory.GetCurrentDirectory() + "/Content/EmailTemplates";

        private readonly Dictionary<NotificationTemplateType, (string, string)> TemplatesMap = new Dictionary<NotificationTemplateType, (string, string)>
        {
            { NotificationTemplateType.VerificationCode, ("Verify It's You", "VerificationCode.html") },
            { NotificationTemplateType.MostSpentCategoryForWeek, ("Your most expensive category for the last week", "WeeklyCategoryReport.html") },
        };


        public NotificationTemplate GetTemplate(NotificationTemplateType templateType, Dictionary<string, string> parameters)
        {
            (string subject, string fileName) = TemplatesMap[templateType];
            return new NotificationTemplate(subject, File.ReadAllText($"{NOTIFICATION_TEMPLATES_FOLDER}/{fileName}"), parameters);
        }
    }
}
