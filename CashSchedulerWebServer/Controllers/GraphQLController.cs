using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CashSchedulerWebServer.Types;
using CashSchedulerWebServer.Authentication;
using GraphQL;
using GraphQL.Types;
using GraphQL.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Cors;
using GraphQL.NewtonsoftJson;
using GraphQL.Validation.Complexity;

namespace CashSchedulerWebServer.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class GraphQLController : ControllerBase
    {
        private readonly ISchema Schema;
        private readonly IDocumentExecuter DocumentExecuter;
        private readonly IEnumerable<IValidationRule> ValidationRules;

        public GraphQLController(
            ISchema schema,
            IDocumentExecuter documentExecuter,
            IEnumerable<IValidationRule> validationRules)
        {
            Schema = schema;
            DocumentExecuter = documentExecuter;
            ValidationRules = validationRules;
        }


        [HttpPost]
        [EnableCors("ReactClient")]
        public async Task<IActionResult> Post([FromBody] GraphQLQuery query, [FromServices] UserContextManager jwtManager, [FromServices] IHttpContextAccessor httpAccessor)
        {
            var userContext = jwtManager.GetUserContext();
            
            httpAccessor.HttpContext.User = userContext.User;

            var result = await DocumentExecuter.ExecuteAsync(new ExecutionOptions
            {
                Schema = Schema,
                Query = query.Query,
                Inputs = query.Variables?.ToInputs(),
                ValidationRules = DocumentValidator.CoreRules.Concat(ValidationRules),
                UserContext = userContext,
                ComplexityConfiguration = new ComplexityConfiguration { MaxDepth = 15 }
            });
            
            IActionResult response;

            if (result.Errors != null && result.Errors.Any())
            {
                response = result.Errors.First().Code switch
                {
                    "authorization" => Unauthorized(result),
                    _ => BadRequest(result),
                };
            }
            else
            {
                response = Ok(result);
            }

            return response;
        }
    }
}
