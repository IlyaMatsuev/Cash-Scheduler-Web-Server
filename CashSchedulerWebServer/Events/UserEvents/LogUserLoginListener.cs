using System;
using System.Threading.Tasks;
using CashSchedulerWebServer.Events.Contracts;
using CashSchedulerWebServer.Exceptions;
using CashSchedulerWebServer.Models;
using CashSchedulerWebServer.WebServices.Contracts;
using CashSchedulerWebServer.WebServices.Salesforce.SObjects;

namespace CashSchedulerWebServer.Events.UserEvents
{
    public class LogUserLoginListener : IEventListener
    {
        public EventAction Action => EventAction.UserLogin;
        private ISalesforceApiWebService SalesforceService { get; }

        public LogUserLoginListener(ISalesforceApiWebService salesforceService)
        {
            SalesforceService = salesforceService;
        }

        public Task Handle(object entity)
        {
            if (entity is not User user)
            {
                throw new CashSchedulerException("Entity should have the type of User", "500");
            }

            user.LastLoginDate = DateTime.Today;

            SalesforceService.UpsertSObject(new SfContact(user));

            return Task.CompletedTask;
        }
    }
}
