using GraphQL.Types;

namespace CashSchedulerWebServer.Types.Inputs
{
    public class UpdateCategoryInputType : InputObjectGraphType
    {
        public UpdateCategoryInputType()
        {
            Name = "UpdateCategoryInput";
            Field<NonNullGraphType<IntGraphType>>("id");
            Field<NonNullGraphType<StringGraphType>>("name");
            Field<StringGraphType>("iconUrl");
        }
    }
}
