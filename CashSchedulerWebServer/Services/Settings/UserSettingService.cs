using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CashSchedulerWebServer.Auth.Contracts;
using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Exceptions;
using CashSchedulerWebServer.Models;
using CashSchedulerWebServer.Services.Contracts;
using CashSchedulerWebServer.Utils;

namespace CashSchedulerWebServer.Services.Settings
{
    public class UserSettingService : IUserSettingService
    {
        private IContextProvider ContextProvider { get; }
        private int UserId { get; }

        public UserSettingService(IContextProvider contextProvider, IUserContext userContext)
        {
            ContextProvider = contextProvider;
            UserId = userContext.GetUserId();
        }
        

        public IEnumerable<UserSetting> GetByUnitName(string unitName = null)
        {
            var settingRepository = ContextProvider.GetRepository<IUserSettingRepository>();
            
            return string.IsNullOrEmpty(unitName) 
                ? settingRepository.GetAll() 
                : settingRepository.GetByUnitName(unitName);
        }

        public Task<UserSetting> Create(UserSetting setting)
        {
            setting.User = ContextProvider.GetRepository<IUserRepository>().GetByKey(UserId);

            return ContextProvider.GetRepository<IUserSettingRepository>().Create(setting);
        }

        public Task<UserSetting> Update(UserSetting setting)
        {
            var settingRepository = ContextProvider.GetRepository<IUserSettingRepository>();
            
            var targetSetting = settingRepository.GetByName(setting.Name);
            if (targetSetting == null)
            {
                return Create(setting);
            }

            if (setting.Value != default)
            {
                targetSetting.Value = setting.Value;
            }

            return settingRepository.Update(targetSetting);
        }

        public async Task<IEnumerable<UserSetting>> Update(List<UserSetting> settings)
        {
            var settingRepository = ContextProvider.GetRepository<IUserSettingRepository>();
            var userRepository = ContextProvider.GetRepository<IUserRepository>();
            var newSettings = new List<UserSetting>();
            var updateSettings = new List<UserSetting>();

            settings.ForEach(setting =>
            {
                var targetSetting = settingRepository.GetByName(setting.Name);
                if (targetSetting == null)
                {
                    ModelValidator.ValidateModelAttributes(setting);
                    setting.User = userRepository.GetByKey(UserId);
                    newSettings.Add(setting);
                }
                else
                {
                    targetSetting.Value = setting.Value;
                    updateSettings.Add(targetSetting);
                }
            });

            return (await settingRepository.Create(newSettings))
                .Concat(await settingRepository.Update(updateSettings));
        }

        public Task<UserSetting> Delete(int id)
        {
            var settingRepository = ContextProvider.GetRepository<IUserSettingRepository>();
            
            var setting = settingRepository.GetByKey(id);
            if (setting == null)
            {
                throw new CashSchedulerException("There is no such setting");
            }

            return settingRepository.Delete(id);
        }
    }
}
