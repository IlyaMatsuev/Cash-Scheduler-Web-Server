using System;
using CashSchedulerWebServer.Models;

namespace CashSchedulerWebServer.WebServices.Salesforce.SObjects
{
    public class SfTransaction : SfObject
    {
        public override string SObjectTypeName => "Transaction__c";

        public string Title__c { get; }

        public double Amount__c { get; }

        public DateTime Date__c { get; }

        public SfContact User__r { get; }

        public SfWallet Wallet__r { get; }

        public SfCategory Category__r { get; }

        public SfTransaction(int id) : base(id) {}

        public SfTransaction(Transaction transaction)
        {
            Id = transaction.Id;
            Title__c = transaction.Title;
            Amount__c = transaction.Amount;
            Date__c = transaction.Date;
            User__r = new SfContact(transaction.User.Id);
            Wallet__r = new SfWallet(transaction.Wallet.Id);
            Category__r = new SfCategory(transaction.Category.Id);
        }

        public SfTransaction(Transaction transaction, int id) : base(id)
        {
            Title__c = transaction.Title;
            Amount__c = transaction.Amount;
            Date__c = transaction.Date;
            User__r = new SfContact(transaction.User.Id);
            Wallet__r = new SfWallet(transaction.Wallet.Id);
            Category__r = new SfCategory(transaction.Category.Id);
        }
    }
}
