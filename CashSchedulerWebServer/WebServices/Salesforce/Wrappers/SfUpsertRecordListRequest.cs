using System.Collections.Generic;
using Newtonsoft.Json;

namespace CashSchedulerWebServer.WebServices.Salesforce.Wrappers
{
    public class SfUpsertRecordListRequest<T> where T : class
    {
        [JsonProperty("allOrNone")]
        public bool AllOrNone { get; set; }
        
        [JsonProperty("records")]
        public List<T> Records { get; set; }

        public SfUpsertRecordListRequest(List<T> records)
        {
            AllOrNone = true;
            Records = records;
        }
    }
}
