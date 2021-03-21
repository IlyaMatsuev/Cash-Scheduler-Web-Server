using CashSchedulerWebServer.Models;

namespace CashSchedulerWebServer.WebServices.Salesforce.SObjects
{
    public class SfCategory : SfObject
    {
        public override string SObjectTypeName => "Category__c";

        public string Name { get; }

        public string IconUrl__c { get; }

        public string TypeName__c { get; }

        public bool? IsCustom__c { get; }

        public SfContact User__r { get; }

        public SfCategory(int id) : base(id) { }

        public SfCategory(Category category)
        {
            Id = category.Id;
            Name = category.Name;
            IconUrl__c = category.IconUrl;
            TypeName__c = category.Type.Name;
            IsCustom__c = category.IsCustom;
            if (category.User != null)
            {
                User__r = new SfContact(category.User.Id);
            }
        }

        public SfCategory(Category category, int id) : base(id)
        {
            Name = category.Name;
            IconUrl__c = category.IconUrl;
            TypeName__c = category.Type.Name;
            IsCustom__c = category.IsCustom;
            if (category.User != null)
            {
                User__r = new SfContact(category.User.Id);
            }
        }
    }
}
