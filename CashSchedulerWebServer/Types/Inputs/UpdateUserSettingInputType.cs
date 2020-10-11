using GraphQL.Types;

namespace CashSchedulerWebServer.Types.Inputs
{
    public class UpdateUserSettingInputType : InputObjectGraphType
    {
        public UpdateUserSettingInputType()
        {
            Name = "UpdateUserSettingInput";
            Field<NonNullGraphType<StringGraphType>>("name");
            Field<StringGraphType>("value");
            Field<StringGraphType>("unitName");
        }
    }
}
