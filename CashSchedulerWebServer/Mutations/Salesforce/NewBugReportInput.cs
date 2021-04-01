using HotChocolate;

namespace CashSchedulerWebServer.Mutations.Salesforce
{
    public class NewBugReportInput
    {
        public string Name { get; set; }

        [GraphQLNonNullType]
        public string Email { get; set; }

        public string Phone { get; set; }

        [GraphQLNonNullType]
        public string Subject { get; set; }

        [GraphQLNonNullType]
        public string Description { get; set; }
    }
}
