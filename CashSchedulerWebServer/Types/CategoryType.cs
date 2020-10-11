using CashSchedulerWebServer.Models;
using GraphQL.Types;

namespace CashSchedulerWebServer.Types
{
    public class CategoryType : ObjectGraphType<Category>
    {
        public CategoryType()
        {
            Field(x => x.Id, nullable: false);
            Field(x => x.Name, nullable: false);
            Field<NonNullGraphType<TransactionTypeType>>("transaction_type", resolve: context => context.Source.Type);
            Field("transaction_type_name", x => x.Type.Name, nullable: false);
            Field<NonNullGraphType<UserType>>("user", resolve: context => context.Source.CreatedBy);
            Field(x => x.IsCustom, nullable: true).Name("is_custom");
            Field(x => x.IconUrl, nullable: true).Name("icon_url");
        }
    }
}
