﻿using System;
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
        
        [GraphQLNonNullType]
        [Authorize(Policy = AuthOptions.AUTH_POLICY)]
        public UserSetting Setting([Service] IContextProvider contextProvider, [GraphQLNonNullType] string name)
        {
            return contextProvider.GetService<IUserSettingService>().GetByName(name);
        }
        
        [Authorize(Policy = AuthOptions.AUTH_POLICY)]
        public IEnumerable<UserSetting>? Settings([Service] IContextProvider contextProvider, string? unitName)
        {
            return contextProvider.GetService<IUserSettingService>().GetByUnitName(unitName);
        }
    }
}
