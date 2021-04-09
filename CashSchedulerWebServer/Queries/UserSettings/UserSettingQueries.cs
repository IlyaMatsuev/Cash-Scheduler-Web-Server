using System;
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
        [GraphQLNonNullType]
        public IEnumerable<string> SettingNames()
        {
            return Enum.GetNames(typeof(Setting.SettingOptions));
        }

        [GraphQLNonNullType]
        public IEnumerable<string> SettingUnits()
        {
            return Enum.GetNames(typeof(Setting.UnitOptions));
        }

        [GraphQLNonNullType]
        public IEnumerable<string> SettingSections()
        {
            return Enum.GetNames(typeof(Setting.SectionOptions));
        }

        [Authorize(Policy = AuthOptions.AUTH_POLICY, Roles = new[] {AuthOptions.USER_ROLE})]
        public IEnumerable<UserSetting>? Settings([Service] IContextProvider contextProvider, string? unitName)
        {
            return contextProvider.GetService<IUserSettingService>().GetByUnitName(unitName);
        }

        public IEnumerable<Language>? Languages([Service] IContextProvider contextProvider)
        {
            return contextProvider.GetService<IUserSettingService>().GetLanguages();
        }
    }
}
