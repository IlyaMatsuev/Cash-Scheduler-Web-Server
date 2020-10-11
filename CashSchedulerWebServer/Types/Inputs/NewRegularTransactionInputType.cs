using GraphQL.Types;

namespace CashSchedulerWebServer.Types.Inputs
{
    public class NewRegularTransactionInputType : InputObjectGraphType
    {
        public NewRegularTransactionInputType()
        {
            Name = "NewRegularTransactionInput";
            Field<StringGraphType>("title");
            Field<NonNullGraphType<IntGraphType>>("categoryId");
            Field<NonNullGraphType<FloatGraphType>>("amount");
            Field<DateTimeGraphType>("date");
            Field<NonNullGraphType<DateTimeGraphType>>("nextTransactionDate");
            Field<NonNullGraphType<StringGraphType>>("interval");
        }
    }
}
