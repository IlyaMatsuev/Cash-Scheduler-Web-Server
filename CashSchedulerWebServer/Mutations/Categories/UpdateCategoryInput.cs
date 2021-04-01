using HotChocolate;

namespace CashSchedulerWebServer.Mutations.Categories
{
    public class UpdateCategoryInput
    {
        public int Id { get; set; }

        [GraphQLNonNullType]
        public string Name { get; set; }

        public string IconUrl { get; set; }
    }
}
