using GraphQL.Types;

namespace CashSchedulerWebServer.Types
{
    public class TransactionTypeType : ObjectGraphType<Models.TransactionType>
    {
        public TransactionTypeType()
        {
            Field(x => x.Name, nullable: false).Name("type_name");
            Field(x => x.IconUrl, nullable: true).Name("image_url");
        }
    }
}
