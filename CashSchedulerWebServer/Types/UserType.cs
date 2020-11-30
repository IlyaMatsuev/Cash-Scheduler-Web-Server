using CashSchedulerWebServer.Models;
using GraphQL.Types;

namespace CashSchedulerWebServer.Types
{
    public class UserType : ObjectGraphType<User>
    {
        public UserType()
        {
            Field(f => f.Id, nullable: false);
            Field(f => f.FirstName, nullable: true);
            Field(f => f.LastName, nullable: true);
            Field(f => f.Email, nullable: false);
            Field(f => f.Balance, nullable: true);
        }
    }
}
