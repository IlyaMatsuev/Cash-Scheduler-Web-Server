﻿using CashSchedulerWebServer.Authentication.Contracts;
using CashSchedulerWebServer.Db.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace CashSchedulerWebServer.Authentication
{
    public class UserContextManager
    {
        private const string TYPE_TOKEN_SEPARATOR = " ";

        private HttpContext HttpContext { get; set; }
        private IContextProvider DbProvider { get; set; }
        private IConfiguration Configuration { get; set; }

        public UserContextManager(
            IHttpContextAccessor httpAccessor,
            IContextProvider contextProvider,
            IConfiguration configuration)
        {
            HttpContext = httpAccessor.HttpContext;
            DbProvider = contextProvider;
            Configuration = configuration;
        }


        public GraphQLUserContext GetUserContext()
        {
            return new GraphQLUserContext(GetPrincipal());
        }

        public ClaimsPrincipal GetPrincipal()
        {
            var claims = new List<Claim>();
            string accessToken = GetTokenFromRequest();

            IEnumerable<Claim> tokenClaims = accessToken.EvaluateToken();
            if (IsUserRegistered(tokenClaims))
            {
                claims.AddRange(tokenClaims);
            }
            else if (bool.Parse(Configuration["IsDevelopment"]) && bool.Parse(Configuration["App:Auth:SkipAuth"]))
            {
                claims.AddRange(GetDevUserClaims());
            }

            return new ClaimsPrincipal(new ClaimsIdentity(claims, AuthOptions.AUTHENTICATION_TYPE));
        }


        private string GetTokenFromRequest()
        {
            string token = string.Empty;
            string authHeader = (string) HttpContext.Request.Headers["Authorization"] ?? string.Empty;

            if (!string.IsNullOrEmpty(authHeader))
            {
                var authParams = authHeader.Split(TYPE_TOKEN_SEPARATOR);

                if (authParams[0] == AuthOptions.AUTHENTICATION_TYPE && authParams.Length > 1)
                {
                    token = authParams[1];
                }
            }

            return token;
        }

        private bool IsUserRegistered(IEnumerable<Claim> claims)
        {
            string expiresIn = claims.FirstOrDefault(claim => claim.Type == "ExpirationDateTime")?.Value ?? string.Empty;
            string id = claims.FirstOrDefault(claim => claim.Type == "Id")?.Value ?? string.Empty;

            return !string.IsNullOrEmpty(expiresIn) && !string.IsNullOrEmpty(id)
                && DateTime.Parse(expiresIn) > DateTime.UtcNow
                && DbProvider.GetRepository<IUserRepository>().GetById(Convert.ToInt32(id)) != null;
        }

        private List<Claim> GetDevUserClaims()
        {
            return new List<Claim>
            {
                new Claim("ExpirationDateTime", DateTime.UtcNow.Add(TimeSpan.FromMinutes(AuthOptions.GetTokenLifetime(AuthOptions.TokenType.ACCESS))).ToString()),
                new Claim("Id", "1")
            };
        }
    }
}
