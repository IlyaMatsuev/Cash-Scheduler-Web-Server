using GraphQL.Types;

namespace CashSchedulerWebServer.Types.Inputs
{
    public class NewCategoryInputType : InputObjectGraphType
    {
        public NewCategoryInputType()
        {
            Name = "NewCategoryInput";
            Field<NonNullGraphType<StringGraphType>>("name");
            Field<NonNullGraphType<StringGraphType>>("transactionTypeName");
            Field<StringGraphType>("iconUrl");
        }
    }
}
