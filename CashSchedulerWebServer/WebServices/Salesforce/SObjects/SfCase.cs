using CashSchedulerWebServer.Models;

namespace CashSchedulerWebServer.WebServices.Salesforce.SObjects
{
    public class SfCase : SfObject
    {
        public override string SObjectTypeName => "Case";

        public string ContactName { get; }

        public string Email { get; }

        public string Phone { get; }

        public string Subject { get; }

        public string Description { get; }

        public SfCase(BugReport bugReport)
        {
            ContactName = bugReport.Name;
            Email = bugReport.Email;
            Phone = bugReport.Phone;
            Subject = bugReport.Subject;
            Description = bugReport.Description;
        }
    }
}
