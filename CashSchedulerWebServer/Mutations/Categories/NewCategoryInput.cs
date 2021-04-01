using HotChocolate;

namespace CashSchedulerWebServer.Mutations.Categories
{
    public class NewCategoryInput
    {
        [GraphQLNonNullType]
        public string Name { get; set; }

        [GraphQLNonNullType]
        public string TransactionTypeName { get; set; }

        public string IconUrl { get; set; }
    }
}
