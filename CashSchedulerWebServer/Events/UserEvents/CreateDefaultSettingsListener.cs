using System.Threading.Tasks;
using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Events.Contracts;
using CashSchedulerWebServer.Exceptions;
using CashSchedulerWebServer.Models;
using CashSchedulerWebServer.Services.Contracts;

namespace CashSchedulerWebServer.Events.UserEvents
{
    public class CreateDefaultSettingsListener : IEventListener
    {
        private IContextProvider ContextProvider { get; }
        public EventAction Action => EventAction.UserRegistered;

        public CreateDefaultSettingsListener(IContextProvider contextProvider)
        {
            ContextProvider = contextProvider;
        }
        
        
        public Task Handle(object entity)
        {
            if (entity is not User user)
            {
                throw new CashSchedulerException("Entity should have the type of User", "500");
            }
            
            return ContextProvider.GetService<IUserSettingService>().CreateDefaultSettings(user);
        }
    }
}
