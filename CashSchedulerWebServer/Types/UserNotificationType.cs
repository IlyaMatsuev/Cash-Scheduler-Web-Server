using CashSchedulerWebServer.Models;
using GraphQL.Types;

namespace CashSchedulerWebServer.Types
{
    public class UserNotificationType : ObjectGraphType<UserNotification>
    {
        public UserNotificationType()
        {
            Field(x => x.Id, nullable: false);
            Field(x => x.Title, nullable: false);
            Field(x => x.Content, nullable: false);
            Field<NonNullGraphType<UserType>>("user", resolve: context => context.Source.CreatedFor);
            Field("read", x => x.IsRead, nullable: false);
        }
    }
}
