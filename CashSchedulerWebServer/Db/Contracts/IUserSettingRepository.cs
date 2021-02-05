using CashSchedulerWebServer.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CashSchedulerWebServer.Db.Contracts
{
    public interface IUserSettingRepository : IRepository<int, UserSetting>
    {
        IEnumerable<UserSetting> GetAllByUnitName(string unitName);
        UserSetting GetByName(string name);
        Task<IEnumerable<UserSetting>> Update(List<UserSetting> settings);
    }
}
