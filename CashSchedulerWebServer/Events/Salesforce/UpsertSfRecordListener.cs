using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CashSchedulerWebServer.Events.Contracts;
using CashSchedulerWebServer.Exceptions;
using CashSchedulerWebServer.Models;
using CashSchedulerWebServer.WebServices.Contracts;
using CashSchedulerWebServer.WebServices.Salesforce.SObjects;

namespace CashSchedulerWebServer.Events.Salesforce
{
    public class UpsertSfRecordListener : IEventListener
    {
        private ISalesforceApiWebService SalesforceService { get; }

        public EventAction Action => EventAction.RecordUpserted;

        public UpsertSfRecordListener(ISalesforceApiWebService salesforceService)
        {
            SalesforceService = salesforceService;
        }


        public Task Handle(object entity)
        {
            switch (entity)
            {
                case User user:
                    SalesforceService.UpsertSObject(new SfContact(user));
                    break;
                case IEnumerable<Wallet> wallets:
                    SalesforceService.UpsertSObjects(wallets.Select(w => new SfWallet(w, w.Id)).Cast<SfObject>().ToList());
                    break;
                case Wallet wallet:
                    SalesforceService.UpsertSObject(new SfWallet(wallet));
                    break;
                case Category category:
                    SalesforceService.UpsertSObject(new SfCategory(category));
                    break;
                case Transaction transaction:
                    SalesforceService.UpsertSObject(new SfTransaction(transaction));
                    break;
                case RegularTransaction recurringTransaction:
                    SalesforceService.UpsertSObject(new SfRecurringTransaction(recurringTransaction));
                    break;
                default:
                    throw new CashSchedulerException("Entity is not of a correct type", "500");
            }

            return Task.CompletedTask;
        }
    }
}
