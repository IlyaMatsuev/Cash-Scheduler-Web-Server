using System;

namespace CashSchedulerWebServer.WebServices.ExchangeRates
{
    public class ConvertCurrencyResponse
    {
        public bool Success { get; set; }
        
        public ConvertCurrencyQuery Query { get; set; }
        
        public ConvertCurrencyInfo Info { get; set; }
        
        public bool Historical { get; set; }
        
        public DateTime Date { get; set; }
        
        public double Result { get; set; }


        public class ConvertCurrencyQuery
        {
            public string From { get; set; }
            
            public string To { get; set; }
            
            public float Amount { get; set; }
        }

        public class ConvertCurrencyInfo
        {
            public float Rate { get; set; }
        }
    }
}
