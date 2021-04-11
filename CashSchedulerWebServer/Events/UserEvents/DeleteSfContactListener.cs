using System.Collections.Generic;
using System.Threading.Tasks;
using CashSchedulerWebServer.Events.Contracts;
using CashSchedulerWebServer.Exceptions;
using CashSchedulerWebServer.Models;
using CashSchedulerWebServer.WebServices.Contracts;
using CashSchedulerWebServer.WebServices.Salesforce.SObjects;

namespace CashSchedulerWebServer.Events.UserEvents
{
    public class DeleteSfContactListener : IEventListener
    {
        private ISalesforceApiWebService SalesforceService { get; }
        public EventAction Action => EventAction.UserDeleted;

        public DeleteSfContactListener(ISalesforceApiWebService salesforceService)
        {
            SalesforceService = salesforceService;
        }


        public Task Handle(object entity)
        {
            if (entity is not User user)
            {
                throw new CashSchedulerException("Entity should have the type of User", "500");
            }

            return SalesforceService.DeleteSObjects(new List<SfObject>
            {
                new SfContact(user.Id)
            });
        }
    }
}
