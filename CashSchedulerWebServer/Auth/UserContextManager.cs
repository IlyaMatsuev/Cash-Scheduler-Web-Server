using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Claims;
using CashSchedulerWebServer.Auth.Contracts;
using CashSchedulerWebServer.Db.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace CashSchedulerWebServer.Auth
{
    public class UserContextManager : IUserContextManager
    {
        public const string ID_CLAIM_TYPE = "Id";
        public const string EXP_DATE_CLAIM_TYPE = "ExpirationDateTime";
        public const string ROLE_CLAIM_TYPE = "Role";

        private HttpContext HttpContext { get; }
        private IConfiguration Configuration { get; }
        private IContextProvider ContextProvider { get; }

        public UserContextManager(
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration,
            IContextProvider contextProvider)
        {
            HttpContext = httpContextAccessor.HttpContext;
            Configuration = configuration;
            ContextProvider = contextProvider;
        }


        public ClaimsPrincipal GetUserPrincipal()
        {
            var claims = new List<Claim>();
            var tokenClaims = GetTokenFromRequest().EvaluateToken();
            if (tokenClaims.IsTokenValid() && UserExists(tokenClaims))
            {
                claims.AddRange(tokenClaims);
            }
            else if (bool.Parse(Configuration["App:Auth:SkipAuth"]))
            {
                claims.AddRange(GetDevUserClaims());
            }

            if (IsSalesforceRequest())
            {
                claims.AddRange(GetSfClaims());
            }

            var identity = new ClaimsIdentity(
                claims,
                Configuration["App:Auth:TokenType"],
                Configuration["App:Auth:TokenName"],
                ROLE_CLAIM_TYPE
            );

            return new ClaimsPrincipal(identity);
        }


        private string GetTokenFromRequest()
        {
            string token = string.Empty;
            string authHeader = (string) HttpContext.Request.Headers["Authorization"] ?? string.Empty;

            if (!string.IsNullOrEmpty(authHeader))
            {
                var authParams = authHeader.Split(AuthOptions.TYPE_TOKEN_SEPARATOR);

                if (authParams[0] == Configuration["App:Auth:TokenType"] && authParams.Length > 1)
                {
                    token = authParams[1];
                }
            }

            return token;
        }

        private bool UserExists(IEnumerable<Claim> claims)
        {
            return ContextProvider.GetRepository<IUserRepository>()
                .GetByKey(Convert.ToInt32(claims.GetUserId())) != null;
        }

        private bool IsSalesforceRequest()
        {
            return HttpContext.Request.Headers.ContainsKey("SfOrigin")
                   && HttpContext.Request.Headers["SfOrigin"] == Configuration["WebServices:SalesforceApi:OrgId"]
                   && HttpContext.Request.Headers["SecurityToken"] == Configuration["WebServices:SalesforceApi:SecurityToken"];
        }

        private IEnumerable<Claim> GetSfClaims()
        {
            return new List<Claim>
            {
                new(ROLE_CLAIM_TYPE, AuthOptions.SALESFORCE_ROLE)
            };
        }

        private IEnumerable<Claim> GetDevUserClaims()
        {
            var fiveMinutesLater = TimeSpan.FromMinutes(
                AuthOptions.GetTokenLifetime(AuthOptions.TokenType.Access, Configuration)
            );

            return new List<Claim>
            {
                new(ID_CLAIM_TYPE, Configuration["App:Auth:DevUserId"]),
                new(
                    EXP_DATE_CLAIM_TYPE,
                    DateTime.UtcNow.Add(fiveMinutesLater).ToString(CultureInfo.InvariantCulture)
                ),
                new(ROLE_CLAIM_TYPE, AuthOptions.USER_ROLE)
            };
        }
    }
}
