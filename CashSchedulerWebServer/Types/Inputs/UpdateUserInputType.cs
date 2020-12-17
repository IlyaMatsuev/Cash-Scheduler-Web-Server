using GraphQL.Types;

namespace CashSchedulerWebServer.Types.Inputs
{
    public class UpdateUserInputType : InputObjectGraphType
    {
        public UpdateUserInputType()
        {
            Name = "UpdateUserInput";
            Field<IntGraphType>("id");
            Field<StringGraphType>("firstName");
            Field<StringGraphType>("lastName");
            Field<FloatGraphType>("balance");
        }
    }
}
