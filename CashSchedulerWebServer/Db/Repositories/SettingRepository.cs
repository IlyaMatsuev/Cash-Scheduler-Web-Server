using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Exceptions;
using CashSchedulerWebServer.Models;

namespace CashSchedulerWebServer.Db.Repositories
{
    public class SettingRepository : ISettingRepository
    {
        private CashSchedulerContext Context { get; }
        
        public SettingRepository(CashSchedulerContext context)
        {
            Context = context;
        }
        
        
        public IEnumerable<Setting> GetAll()
        {
            return Context.Settings;
        }

        public Setting GetByKey(string name)
        {
            return Context.Settings.FirstOrDefault(s => s.Name == name);
        }

        public Task<Setting> Create(Setting setting)
        {
            throw new CashSchedulerException("It's forbidden to create new settings");
        }

        public Task<Setting> Update(Setting setting)
        {
            throw new CashSchedulerException("It's forbidden to update settings");
        }

        public Task<Setting> Delete(string name)
        {
            throw new CashSchedulerException("It's forbidden to delete settings");
        }
    }
}
