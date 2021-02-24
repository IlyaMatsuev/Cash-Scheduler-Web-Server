using HotChocolate;

namespace CashSchedulerWebServer.Mutations.Settings
{
    public class UpdateUserSettingInput
    {
        [GraphQLNonNullType]
        public string Name { get; set; }

        [GraphQLNonNullType]
        public string Value { get; set; }
    }
}
