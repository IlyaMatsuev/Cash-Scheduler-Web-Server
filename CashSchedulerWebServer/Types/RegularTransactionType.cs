using CashSchedulerWebServer.Models;
using GraphQL.Types;

namespace CashSchedulerWebServer.Types
{
    public class RegularTransactionType : ObjectGraphType<RegularTransaction>
    {
        public RegularTransactionType()
        {
            Field(x => x.Id, nullable: false);
            Field(x => x.Title, nullable: true);
            Field(x => x.Amount, nullable: false);
            Field(x => x.Date, nullable: false);
            Field<NonNullGraphType<UserType>>("user", resolve: context => context.Source.CreatedBy);
            Field<NonNullGraphType<CategoryType>>("category", resolve: context => context.Source.TransactionCategory);
            Field("next_transaction_date", x => x.NextTransactionDate, nullable: false);
            Field("interval", x => x.Interval, nullable: false);

        }
    }
}
