using GraphQL;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace CashSchedulerWebServer.Utils
{
    public class ModelValidator
    {
        public static void ValidateModelAttributes<T>(T model)
        {
            var validationErrors = new List<ValidationResult>();
            if (!Validator.TryValidateObject(model, new ValidationContext(model), validationErrors, true))
            {
                throw new ExecutionError(validationErrors.First().ErrorMessage);
            }
        }
    }
}
