using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CashSchedulerWebServer.Auth;
using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Models;
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
        [Authorize(Policy = AuthOptions.AUTH_POLICY)]
        public Task<UserSetting> UpdateUserSetting([Service] IContextProvider contextProvider, [GraphQLNonNullType] UpdateUserSettingInput setting)
        {
            return contextProvider.GetRepository<IUserSettingRepository>().Update(new UserSetting
            {
                Name = setting.Name,
                Value = setting.Value,
                UnitName = setting.UnitName
            });
        }
        
        [Authorize(Policy = AuthOptions.AUTH_POLICY)]
        public Task<IEnumerable<UserSetting>?> UpdateUserSettings(
            [Service] IContextProvider contextProvider,
            [GraphQLNonNullType] IEnumerable<UpdateUserSettingInput> settings)
        {
            return contextProvider.GetRepository<IUserSettingRepository>().Update(settings.Select(setting => new UserSetting
            {
                Name = setting.Name,
                Value = setting.Value,
                UnitName = setting.UnitName
            }).ToList());
        }
    }
}
