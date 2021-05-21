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

        public IEnumerable<Language> GetLanguages()
        {
            return ContextProvider.GetRepository<ILanguageRepository>().GetAll();
        }

        public Task<IEnumerable<UserSetting>> CreateDefaultSettings(User user)
        {
            var targetUser = ContextProvider.GetRepository<IUserRepository>().GetByKey(user.Id);

            var allSettings = ContextProvider.GetRepository<ISettingRepository>().GetAll();

            static string GetValue(Setting setting)
            {
                if (setting.Name == Setting.SettingOptions.Language.ToString())
                {
                    return Language.DEFAULT_LANGUAGE_ABBREVIATION;
                }
                if (setting.Name == Setting.SettingOptions.DarkTheme.ToString())
                {
                    return false.ToString().ToLower();
                }
                return true.ToString().ToLower();
            }

            return ContextProvider.GetRepository<IUserSettingRepository>()
                .Create(allSettings.Select(setting => new UserSetting
                {
                    User = targetUser,
                    Setting = setting,
                    Value = GetValue(setting)
                }).ToList());
        }

        public Task<UserSetting> Create(UserSetting setting)
        {
            setting.User = ContextProvider.GetRepository<IUserRepository>().GetByKey(UserId);

            setting.Setting = ContextProvider.GetRepository<ISettingRepository>().GetByKey(setting.Name);

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
            var settingRepository = ContextProvider.GetRepository<ISettingRepository>();
            var userSettingRepository = ContextProvider.GetRepository<IUserSettingRepository>();
            var userRepository = ContextProvider.GetRepository<IUserRepository>();
            var newSettings = new List<UserSetting>();
            var updateSettings = new List<UserSetting>();

            settings.ForEach(setting =>
            {
                var targetSetting = userSettingRepository.GetByName(setting.Name);
                if (targetSetting == null)
                {
                    setting.User = userRepository.GetByKey(UserId);
                    setting.Setting = settingRepository.GetByKey(setting.Name);
                    ModelValidator.ValidateModelAttributes(setting);
                    newSettings.Add(setting);
                }
                else
                {
                    if (setting.Value != default)
                    {
                        targetSetting.Value = setting.Value;                        
                    }
                    updateSettings.Add(targetSetting);
                }
            });

            return (await userSettingRepository.Create(newSettings))
                .Concat(await userSettingRepository.Update(updateSettings));
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