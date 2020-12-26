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
            Field<NonNullGraphType<DateGraphType>>("date", resolve: context => context.Source.Date);
            Field<NonNullGraphType<UserType>>("user", resolve: context => context.Source.CreatedBy);
            Field<NonNullGraphType<CategoryType>>("category", resolve: context => context.Source.TransactionCategory);
            Field<NonNullGraphType<DateGraphType>>("nextTransactionDate", resolve: context => context.Source.NextTransactionDate);
            Field(x => x.Interval, nullable: false);
        }
    }
}
