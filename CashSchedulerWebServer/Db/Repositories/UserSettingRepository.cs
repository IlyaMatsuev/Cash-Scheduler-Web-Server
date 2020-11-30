using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Exceptions;
using CashSchedulerWebServer.Models;
using CashSchedulerWebServer.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CashSchedulerWebServer.Db.Repositories
{
    public class UserSettingRepository : IUserSettingRepository
    {
        private CashSchedulerContext Context { get; set; }
        private IContextProvider ContextProvider { get; set; }
        private ClaimsPrincipal User { get; set; }
        private int? UserId => Convert.ToInt32(User?.Claims.FirstOrDefault(claim => claim.Type == "Id")?.Value ?? "-1");

        public UserSettingRepository(CashSchedulerContext context, IHttpContextAccessor httpAccessor, IContextProvider contextProvider)
        {
            Context = context;
            ContextProvider = contextProvider;
            User = httpAccessor.HttpContext.User;
        }


        public IEnumerable<UserSetting> GetAll()
        {
            return Context.UserSettings.Where(s => s.SettingFor.Id == UserId)
                .Include(s => s.SettingFor);
        }

        public IEnumerable<UserSetting> GetAllByUnitName(string unitName)
        {
            if (string.IsNullOrEmpty(unitName))
            {
                return GetAll();
            }
            return Context.UserSettings.Where(s => s.UnitName == unitName && s.SettingFor.Id == UserId)
                .Include(s => s.SettingFor);
        }

        public UserSetting GetById(int id)
        {
            return Context.UserSettings.Where(s => s.Id == id && s.SettingFor.Id == UserId)
                .Include(s => s.SettingFor)
                .FirstOrDefault();
        }

        public UserSetting GetByName(string name)
        {
            return Context.UserSettings.Where(s => s.Name == name && s.SettingFor.Id == UserId)
                .Include(s => s.SettingFor)
                .FirstOrDefault();
        }

        public async Task<UserSetting> Create(UserSetting setting)
        {
            ModelValidator.ValidateModelAttributes(setting);

            setting.SettingFor = ContextProvider.GetRepository<IUserRepository>().GetById((int)UserId);

            Context.UserSettings.Add(setting);
            await Context.SaveChangesAsync();

            return GetById(setting.Id);
        }

        public async Task<UserSetting> Update(UserSetting setting)
        {
            var targetSetting = GetByName(setting.Name);
            if (targetSetting == null)
            {
                return await Create(setting);
            }

            if (setting.Value != default)
            {
                targetSetting.Value = setting.Value;
            }

            ModelValidator.ValidateModelAttributes(targetSetting);

            Context.UserSettings.Update(targetSetting);
            await Context.SaveChangesAsync();

            return targetSetting;
        }

        public async Task<IEnumerable<UserSetting>> Update(List<UserSetting> settings)
        {
            List<UserSetting> newSettings = new List<UserSetting>();
            List<UserSetting> updateSettings = new List<UserSetting>();

            settings.ForEach(setting =>
            {
                var targetSetting = Context.UserSettings.FirstOrDefault(s => s.Name == setting.Name && s.SettingFor.Id == UserId);
                if (targetSetting == null)
                {
                    ModelValidator.ValidateModelAttributes(setting);
                    setting.SettingFor = ContextProvider.GetRepository<IUserRepository>().GetById((int)UserId);
                    newSettings.Add(setting);
                }
                else
                {
                    targetSetting.Value = setting.Value;
                    updateSettings.Add(targetSetting);
                }
            });

            Context.UserSettings.AddRange(newSettings);
            Context.UserSettings.UpdateRange(updateSettings);

            await Context.SaveChangesAsync();

            return newSettings.Concat(updateSettings);
        }

        public async Task<UserSetting> Delete(int settingId)
        {
            var setting = GetById(settingId);
            if (setting == null)
            {
                throw new CashSchedulerException("There is no such setting");
            }

            Context.UserSettings.Remove(setting);
            await Context.SaveChangesAsync();

            return setting;
        }
    }
}
