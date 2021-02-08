using System.Collections.Generic;
using System.Threading.Tasks;
using CashSchedulerWebServer.Models;

namespace CashSchedulerWebServer.Services.Contracts
{
    public interface IUserSettingService : IService<int, UserSetting>
    {
        IEnumerable<UserSetting> GetByUnitName(string unitName);
        Task<IEnumerable<UserSetting>> Update(List<UserSetting> settings);
    }
}
