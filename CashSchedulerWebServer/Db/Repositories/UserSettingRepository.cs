using CashSchedulerWebServer.Db.Contracts;
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
        private int UserId { get; }

        public UserSettingRepository(CashSchedulerContext context, IUserContext userContext)
        {
            Context = context;
            UserId = userContext.GetUserId();
        }


        public UserSetting GetByKey(int id)
        {
            return Context.UserSettings
                .Where(s => s.Id == id && s.User.Id == UserId)
                .Include(s => s.User)
                .FirstOrDefault();
        }
        
        public IEnumerable<UserSetting> GetAll()
        {
            return Context.UserSettings
                .Where(s => s.User.Id == UserId)
                .Include(s => s.User)
                .Include(s => s.Setting);
        }

        public IEnumerable<UserSetting> GetByUnitName(string unitName)
        {
            return Context.UserSettings
                .Where(s => s.Setting.UnitName == unitName && s.User.Id == UserId)
                .Include(s => s.User)
                .Include(s => s.Setting);
        }

        public UserSetting GetByName(string name)
        {
            return Context.UserSettings.Where(s => s.Setting.Name == name && s.User.Id == UserId)
                .Include(s => s.User)
                .Include(s => s.Setting)
                .FirstOrDefault();
        }

        public async Task<UserSetting> Create(UserSetting setting)
        {
            ModelValidator.ValidateModelAttributes(setting);

            await Context.UserSettings.AddAsync(setting);
            await Context.SaveChangesAsync();

            return GetByKey(setting.Id);
        }

        public async Task<IEnumerable<UserSetting>> Create(List<UserSetting> settings)
        {
            await Context.UserSettings.AddRangeAsync(settings);
            await Context.SaveChangesAsync();

            return settings;
        }

        public async Task<UserSetting> Update(UserSetting setting)
        {
            ModelValidator.ValidateModelAttributes(setting);

            Context.UserSettings.Update(setting);
            await Context.SaveChangesAsync();

            return GetByKey(setting.Id);
        }

        public async Task<IEnumerable<UserSetting>> Update(List<UserSetting> settings)
        {
            Context.UserSettings.UpdateRange(settings);
            await Context.SaveChangesAsync();

            return settings;
        }

        public async Task<UserSetting> Delete(int id)
        {
            var setting = GetByKey(id);

            Context.UserSettings.Remove(setting);
            await Context.SaveChangesAsync();

            return setting;
        }

        public IEnumerable<UserSetting> DeleteByUserId(int userId)
        {
            var settings = Context.UserSettings.Where(c => c.User.Id == userId);

            Context.UserSettings.RemoveRange(settings);
            Context.SaveChanges();

            return settings;
        }
    }
}
