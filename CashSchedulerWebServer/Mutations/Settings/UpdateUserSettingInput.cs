using HotChocolate;

namespace CashSchedulerWebServer.Mutations.Settings
{
    public class UpdateUserSettingInput
    {
        [GraphQLNonNullType]
        public string Name { get; set; }
        
        public string Value { get; set; }
        
        [GraphQLNonNullType]
        public string UnitName { get; set; }
    }
}
