using CashSchedulerWebServer.Models;

namespace CashSchedulerWebServer.WebServices.Salesforce.SObjects
{
    public class SfWallet : SfObject
    {
        public override string SObjectTypeName => "Wallet__c";

        public string Name { get; }

        public double? Balance__c { get; }

        public string CurrencyName__c { get; }

        public bool? IsDefault__c { get; }

        public SfContact User__r { get; }

        public SfWallet(int id) : base(id) {}

        public SfWallet(Wallet wallet)
        {
            Id = wallet.Id;
            Name = wallet.Name;
            Balance__c = wallet.Balance;
            CurrencyName__c = wallet.Currency.Abbreviation;
            IsDefault__c = wallet.IsDefault;
            User__r = new SfContact(wallet.User.Id);
        }

        public SfWallet(Wallet wallet, int id) : base(id)
        {
            Name = wallet.Name;
            Balance__c = wallet.Balance;
            CurrencyName__c = wallet.Currency.Abbreviation;
            IsDefault__c = wallet.IsDefault;
            User__r = new SfContact(wallet.User.Id);
        }
    }
}
