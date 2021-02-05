using System;
using System.Collections.Generic;

namespace CashSchedulerWebServer.WebServices.ExchangeRates
{
    public class ExchangeRatesResponse
    {
        public bool Success { get; set; }
        
        public string Base { get; set; }
        
        public DateTime Date { get; set; }
        
        public Dictionary<string, float> Rates { get; set; }
    }
}
