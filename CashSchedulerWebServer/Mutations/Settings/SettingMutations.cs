using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CashSchedulerWebServer.Auth;
using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Models;
using CashSchedulerWebServer.Services.Contracts;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Types;

#nullable enable

namespace CashSchedulerWebServer.Mutations.Settings
{
    [ExtendObjectType(Name = "Mutation")]
    public class SettingMutations
    {
        [GraphQLNonNullType]
        [Authorize(Policy = AuthOptions.AUTH_POLICY, Roles = new[] {AuthOptions.USER_ROLE})]
        public Task<UserSetting> UpdateUserSetting(
            [Service] IContextProvider contextProvider,
            [GraphQLNonNullType] UpdateUserSettingInput setting)
        {
            return contextProvider.GetService<IUserSettingService>().Update(new UserSetting
            {
                Name = setting.Name,
                Value = setting.Value
            });
        }

        [Authorize(Policy = AuthOptions.AUTH_POLICY, Roles = new[] {AuthOptions.USER_ROLE})]
        public Task<IEnumerable<UserSetting>?> UpdateUserSettings(
            [Service] IContextProvider contextProvider,
            [GraphQLNonNullType] IEnumerable<UpdateUserSettingInput> settings)
        {
            return contextProvider.GetService<IUserSettingService>().Update(settings.Select(setting => new UserSetting
            {
                Name = setting.Name,
                Value = setting.Value
            }).ToList());
        }
    }
}
