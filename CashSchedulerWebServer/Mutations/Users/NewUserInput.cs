using HotChocolate;

namespace CashSchedulerWebServer.Mutations.Users
{
    public class NewUserInput
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }

        public double? Balance { get; set; }

        [GraphQLNonNullType]
        public string Email { get; set; }

        [GraphQLNonNullType]
        public string Password { get; set; }
    }
}
