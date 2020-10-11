using GraphQL.Types;

namespace CashSchedulerWebServer.Types.Inputs
{
    public class UpdateRegularTransactionInputType : InputObjectGraphType
    {
        public UpdateRegularTransactionInputType()
        {
            Name = "UpdateRegularTransactionInput";
            Field<NonNullGraphType<IntGraphType>>("id");
            Field<StringGraphType>("title");
            Field<FloatGraphType>("amount");
        }
    }
}
