using CashSchedulerWebServer.Models;
using System.Collections.Generic;

namespace CashSchedulerWebServer.Db.Contracts
{
    interface IUserSettingRepository : IRepository<UserSetting>
    {
        IEnumerable<UserSetting> GetAllByUnitName(string unitName);
        UserSetting GetByName(string name);
    }
}
