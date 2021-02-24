using System.Collections.Generic;
using System.Threading.Tasks;
using CashSchedulerWebServer.Models;

namespace CashSchedulerWebServer.Services.Contracts
{
    public interface IUserSettingService : IService<int, UserSetting>
    {
        IEnumerable<UserSetting> GetByUnitName(string unitName);
        UserSetting GetByName(string name);
        Task<IEnumerable<UserSetting>> CreateDefaultSettings(User user);
        Task<IEnumerable<UserSetting>> Update(List<UserSetting> settings);
    }
}
