using System.Threading.Tasks;
using CashSchedulerWebServer.Events.Contracts;
using CashSchedulerWebServer.Exceptions;
using CashSchedulerWebServer.Models;
using CashSchedulerWebServer.WebServices.Contracts;
using CashSchedulerWebServer.WebServices.Salesforce.SObjects;

namespace CashSchedulerWebServer.Events.Salesforce
{
    public class DeleteSfRecordListener : IEventListener
    {
        private ISalesforceApiWebService SalesforceService { get; }

        public EventAction Action => EventAction.RecordDeleted;

        public DeleteSfRecordListener(ISalesforceApiWebService salesforceService)
        {
            SalesforceService = salesforceService;
        }


        public Task Handle(object entity)
        {
            switch (entity)
            {
                case Wallet wallet:
                    SalesforceService.DeleteSObject(new SfWallet(wallet.Id));
                    break;
                case Category category:
                    SalesforceService.DeleteSObject(new SfCategory(category.Id));
                    break;
                case Transaction transaction:
                    SalesforceService.DeleteSObject(new SfTransaction(transaction.Id));
                    break;
                case RegularTransaction recurringTransaction:
                    SalesforceService.DeleteSObject(new SfRecurringTransaction(recurringTransaction.Id));
                    break;
                default:
                    throw new CashSchedulerException("Entity is not of a correct type", "500");
            }

            return Task.CompletedTask;
        }
    }
}
