using GraphQL.Types;

namespace CashSchedulerWebServer.Types.Inputs
{
    public class UpdateTransactionInputType : InputObjectGraphType
    {
        public UpdateTransactionInputType()
        {
            Name = "UpdateTransactionInput";
            Field<NonNullGraphType<IntGraphType>>("id");
            Field<StringGraphType>("title");
            Field<FloatGraphType>("amount");
            Field<DateTimeGraphType>("date");
        }
    }
}
