using CashSchedulerWebServer.Models;
using GraphQL.Types;

namespace CashSchedulerWebServer.Types
{
    public class UserType : ObjectGraphType<User>
    {
        public UserType()
        {
            Field(f => f.Id, nullable: false);
            Field("first_name", f => f.FirstName, nullable: true);
            Field("last_name", f => f.LastName, nullable: true);
            Field(f => f.Email, nullable: false);
            Field(f => f.Balance, nullable: true);
        }
    }
}
