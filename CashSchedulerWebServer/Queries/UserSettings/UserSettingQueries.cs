using System.Collections.Generic;
using CashSchedulerWebServer.Auth;
using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Models;
using CashSchedulerWebServer.Services.Contracts;
using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Types;

#nullable enable

namespace CashSchedulerWebServer.Queries.UserSettings
{
    [ExtendObjectType(Name = "Query")]
    public class UserSettingQueries
    {
        [Authorize(Policy = AuthOptions.AUTH_POLICY)]
        public IEnumerable<UserSetting>? Settings([Service] IContextProvider contextProvider, string? unitName)
        {
            return contextProvider.GetService<IUserSettingService>().GetByUnitName(unitName);
        }
    }
}
