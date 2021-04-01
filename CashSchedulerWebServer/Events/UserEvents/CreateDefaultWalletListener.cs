using System.Threading.Tasks;
using CashSchedulerWebServer.Db.Contracts;
using CashSchedulerWebServer.Events.Contracts;
using CashSchedulerWebServer.Exceptions;
using CashSchedulerWebServer.Models;
using CashSchedulerWebServer.Services.Contracts;
using CashSchedulerWebServer.WebServices.Contracts;
using CashSchedulerWebServer.WebServices.Salesforce.SObjects;

namespace CashSchedulerWebServer.Events.UserEvents
{
    public class CreateDefaultWalletListener : IEventListener
    {
        private IContextProvider ContextProvider { get; }
        private ISalesforceApiWebService SalesforceService { get; }
        public EventAction Action => EventAction.UserRegistered;

        public CreateDefaultWalletListener(IContextProvider contextProvider, ISalesforceApiWebService salesforceService)
        {
            ContextProvider = contextProvider;
            SalesforceService = salesforceService;
        }


        public async Task Handle(object entity)
        {
            if (entity is not User user)
            {
                throw new CashSchedulerException("Entity should have the type of User", "500");
            }

            var wallet = await ContextProvider.GetService<IWalletService>().CreateDefault(user);
            SalesforceService.RunWithDelay(new SfWallet(wallet), 10, (s, o) => s.UpsertSObject(o));
        }
    }
}
