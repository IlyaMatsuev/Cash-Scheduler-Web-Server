using System.Collections.Generic;

namespace CashSchedulerWebServer.Notifications
{
    public class NotificationTemplate
    {
        public string Subject { get; }
        public string Body { get; private set; }

        public NotificationTemplate(string subject, string body, Dictionary<string, string> parameters)
        {
            Subject = subject;
            Body = body;
            ApplyParams(parameters);
        }


        private void ApplyParams(Dictionary<string, string> parameters)
        {
            if (parameters == null)
            {
                return;
            }

            foreach (string name in parameters.Keys)
            {
                string varKey = "{{{" + name + "}}}";
                string varValue = parameters[name];
                Body = Body.Replace(varKey, varValue);
            }
        }
    }
}
