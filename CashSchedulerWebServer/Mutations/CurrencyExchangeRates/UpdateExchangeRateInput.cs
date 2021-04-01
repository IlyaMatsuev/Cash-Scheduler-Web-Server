using System;

namespace CashSchedulerWebServer.Mutations.CurrencyExchangeRates
{
    public class UpdateExchangeRateInput
    {
        public int Id { get; set; }

        public string SourceCurrencyAbbreviation { get; set; }

        public string TargetCurrencyAbbreviation { get; set; }

        public float? ExchangeRate { get; set; }

        public DateTime? ValidFrom { get; set; }

        public DateTime? ValidTo { get; set; }
    }
}
