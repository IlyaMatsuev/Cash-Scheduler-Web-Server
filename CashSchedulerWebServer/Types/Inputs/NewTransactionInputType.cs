using GraphQL.Types;

namespace CashSchedulerWebServer.Types.Inputs
{
    public class NewTransactionInputType : InputObjectGraphType
    {
        public NewTransactionInputType()
        {
            Name = "NewTransactionInput";
            Field<StringGraphType>("title");
            Field<NonNullGraphType<IntGraphType>>("categoryId");
            Field<NonNullGraphType<FloatGraphType>>("amount");
            Field<DateTimeGraphType>("date");
        }
    }
}
