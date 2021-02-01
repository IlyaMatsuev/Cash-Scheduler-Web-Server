using System;
using System.ComponentModel.DataAnnotations;

namespace CashSchedulerWebServer.Models.Validations
{
    public class GreaterThanTodayAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if ((DateTime) value <= DateTime.Today)
            {
                return new ValidationResult(
                    ErrorMessage ?? "Date must be greater than today",
                    new[] {validationContext.MemberName}
                );
            }

            return ValidationResult.Success;
        }
    }
}
