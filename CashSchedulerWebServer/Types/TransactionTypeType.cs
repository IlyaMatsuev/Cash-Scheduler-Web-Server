using GraphQL.Types;

namespace CashSchedulerWebServer.Types
{
    public class TransactionTypeType : ObjectGraphType<Models.TransactionType>
    {
        public TransactionTypeType()
        {
            Field(x => x.Name, nullable: false).Name("typeName");
            Field(x => x.IconUrl, nullable: true);
        }
    }
}
