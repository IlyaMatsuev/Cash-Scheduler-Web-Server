using System;
using HotChocolate;

namespace CashSchedulerWebServer.Mutations.CurrencyExchangeRates
{
    public class NewExchangeRateInput
    {
        [GraphQLNonNullType]
        public string SourceCurrencyAbbreviation { get; set; }

        [GraphQLNonNullType]
        public string TargetCurrencyAbbreviation { get; set; }

        public float ExchangeRate { get; set; }

        public DateTime ValidFrom { get; set; }

        public DateTime ValidTo { get; set; }
    }
}
