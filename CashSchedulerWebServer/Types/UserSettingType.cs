using CashSchedulerWebServer.Models;
using GraphQL.Types;

namespace CashSchedulerWebServer.Types
{
    public class UserSettingType : ObjectGraphType<UserSetting>
    {
        public UserSettingType()
        {
            Field(x => x.Id, nullable: false);
            Field(x => x.Name, nullable: false);
            Field(x => x.UnitName, nullable: false);
            Field(x => x.Value, nullable: true);
            Field<NonNullGraphType<UserType>>("user", resolve: context => context.Source.SettingFor);
        }
    }
}
