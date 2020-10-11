using GraphQL.Types;

namespace CashSchedulerWebServer.Types.Inputs
{
    public class NewUserInputType : InputObjectGraphType
    {
        public NewUserInputType()
        {
            Name = "NewUserInput";
            Field<StringGraphType>("firstName");
            Field<StringGraphType>("lastName");
            Field<FloatGraphType>("balance");
            Field<NonNullGraphType<StringGraphType>>("email");
            Field<NonNullGraphType<StringGraphType>>("password");
        }
    }
}
