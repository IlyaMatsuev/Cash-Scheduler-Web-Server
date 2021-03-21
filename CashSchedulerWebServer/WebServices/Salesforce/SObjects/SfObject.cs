using Newtonsoft.Json;

namespace CashSchedulerWebServer.WebServices.Salesforce.SObjects
{
    public class SfObject
    {
        [JsonIgnore]
        public virtual string SObjectTypeName => "SObject";

        [JsonIgnore]
        public int Id { get; protected set; }

        [JsonProperty(Order = -2)]
        public SfObjectAttributes Attributes { get; protected set; }

        public int? CashSchedulerId__c { get; }

        public SfObject()
        {
            // ReSharper disable once VirtualMemberCallInConstructor
            Attributes = new SfObjectAttributes(SObjectTypeName);
        }

        public SfObject(int id)
        {
            Id = id;
            CashSchedulerId__c = id;
            // ReSharper disable once VirtualMemberCallInConstructor
            Attributes = new SfObjectAttributes(SObjectTypeName);
        }

        public class SfObjectAttributes
        {
            [JsonProperty("type")]
            public string Type { get; set; }

            public SfObjectAttributes(string type)
            {
                Type = type;
            }
        }
    }
}
