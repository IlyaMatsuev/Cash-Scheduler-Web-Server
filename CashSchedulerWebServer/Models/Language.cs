using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CashSchedulerWebServer.Models
{
    public class Language
    {
        [NotMapped]
        public static string DEFAULT_LANGUAGE_ABBREVIATION = "en";


        [Key]
        public string Abbreviation { get; set; }

        [Required(ErrorMessage = "Name is required for language")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Icon url is required for language")]
        public string IconUrl { get; set; }
    }
}
