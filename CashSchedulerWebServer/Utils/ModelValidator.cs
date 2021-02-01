using CashSchedulerWebServer.Exceptions;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace CashSchedulerWebServer.Utils
{
    public static class ModelValidator
    {
        public static void ValidateModelAttributes<T>(T model)
        {
            var validationErrors = new List<ValidationResult>();
            if (Validator.TryValidateObject(model, new ValidationContext(model), validationErrors, true))
            {
                return;
            }
            
            var error = validationErrors.First();
            throw new CashSchedulerException(
                error.ErrorMessage, 
                error.MemberNames.Select(field => field.Substring(0, 1).ToLower() + field.Substring(1)).ToArray()
            );
        }
    }
}
