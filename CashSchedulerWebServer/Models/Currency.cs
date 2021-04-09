using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HotChocolate;

namespace CashSchedulerWebServer.Models
{
    public class Currency
    {
        [NotMapped]
        public const string DEFAULT_CURRENCY_ABBREVIATION_USD = "USD";

        [NotMapped]
        public const string DEFAULT_CURRENCY_ABBREVIATION_EUR = "EUR";

        [NotMapped]
        private const string DEFAULT_ICON_URL = "/static/icons/currencies/unknown.png";


        [Key]
        public string Abbreviation { get; set; }

        [GraphQLNonNullType]
        public string Name { get; set; }

        public string IconUrl
        {
            get => iconUrl;
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    iconUrl = value;
                }
            }
        }

        [NotMapped]
        private string iconUrl = DEFAULT_ICON_URL;
    }
}
