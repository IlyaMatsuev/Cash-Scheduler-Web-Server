using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Exceptions;
using CashSchedulerWebServer.Models;
using CashSchedulerWebServer.Utils;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CashSchedulerWebServer.Auth.Contracts;

namespace CashSchedulerWebServer.Db.Repositories
{
    public class UserSettingRepository : IUserSettingRepository
    {
        private CashSchedulerContext Context { get; }
        private IContextProvider ContextProvider { get; }
        private int UserId { get; }

        public UserSettingRepository(CashSchedulerContext context, IUserContext userContext, IContextProvider contextProvider)
        {
            Context = context;
            ContextProvider = contextProvider;
            UserId = userContext.GetUserId();
        }


        public IEnumerable<UserSetting> GetAll()
        {
            return Context.UserSettings.Where(s => s.User.Id == UserId)
                .Include(s => s.User);
        }

        public IEnumerable<UserSetting> GetAllByUnitName(string unitName)
        {
            if (string.IsNullOrEmpty(unitName))
            {
                return GetAll();
            }
            return Context.UserSettings.Where(s => s.UnitName == unitName && s.User.Id == UserId)
                .Include(s => s.User);
        }

        public UserSetting GetById(int id)
        {
            return Context.UserSettings.Where(s => s.Id == id && s.User.Id == UserId)
                .Include(s => s.User)
                .FirstOrDefault();
        }

        public UserSetting GetByName(string name)
        {
            return Context.UserSettings.Where(s => s.Name == name && s.User.Id == UserId)
                .Include(s => s.User)
                .FirstOrDefault();
        }

        public async Task<UserSetting> Create(UserSetting setting)
        {
            ModelValidator.ValidateModelAttributes(setting);

            setting.User = ContextProvider.GetRepository<IUserRepository>().GetById(UserId);

            Context.UserSettings.Add(setting);
            await Context.SaveChangesAsync();

            return GetById(setting.Id);
        }

        public async Task<IEnumerable<UserSetting>> Create(List<UserSetting> settings)
        {
            await Context.UserSettings.AddRangeAsync(settings);
            await Context.SaveChangesAsync();

            return settings;
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
            var newSettings = new List<UserSetting>();
            var updateSettings = new List<UserSetting>();

            settings.ForEach(setting =>
            {
                var targetSetting = Context.UserSettings.FirstOrDefault(s => s.Name == setting.Name && s.User.Id == UserId);
                if (targetSetting == null)
                {
                    ModelValidator.ValidateModelAttributes(setting);
                    setting.User = ContextProvider.GetRepository<IUserRepository>().GetById(UserId);
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
