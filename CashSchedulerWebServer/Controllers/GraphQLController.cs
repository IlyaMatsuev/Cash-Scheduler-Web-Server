using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CashSchedulerWebServer.Types;
using CashSchedulerWebServer.Authentication;
using GraphQL;
using GraphQL.Types;
using GraphQL.Validation;
using Microsoft.AspNetCore.Mvc;
using CashSchedulerWebServer.Exceptions;
using Microsoft.AspNetCore.Http;

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
        public async Task<IActionResult> Post([FromBody] GraphQLQuery query, [FromServices] UserContextManager jwtManager, [FromServices] IHttpContextAccessor httpAccessor)
        {
            var userContext = jwtManager.GetUserContext();
            
            httpAccessor.HttpContext.User = userContext.User;

            var result = await DocumentExecuter.ExecuteAsync(new ExecutionOptions
            {
                Schema = Schema,
                Query = query.Query,
                Inputs = query.Variables?.ToObject<Inputs>(),
                ValidationRules = DocumentValidator.CoreRules().Concat(ValidationRules),
                UserContext = userContext
            });
            
            IActionResult response;

            if (result.Errors != null && result.Errors.Any())
            {
                switch (result.Errors.First().Code)
                {
                    case "authorization":
                        response = Unauthorized(result);
                        break;
                    case "redirect_login":
                        response = Redirect("/login");
                        break;
                    default:
                        response = BadRequest(result);
                        break;
                }
            }
            else
            {
                response = Ok(result);
            }

            return response;
        }
    }
}
