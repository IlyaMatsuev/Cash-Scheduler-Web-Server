using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using CashSchedulerWebServer.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace CashSchedulerWebServer.Auth
{
    public static class JwtTokenExtensions
    {
        public static (string token, DateTime expiresIn) GenerateToken(this User user, AuthOptions.TokenType tokenType, IConfiguration configuration)
        {
            var now = DateTime.UtcNow;
            var expiresIn = now.AddMinutes(AuthOptions.GetTokenLifetime(tokenType, configuration));
            var claims = new List<Claim>
            {
                new (UserContextManager.ID_CLAIM_TYPE, user.Id.ToString()),
                new (UserContextManager.EXP_DATE_CLAIM_TYPE, expiresIn.ToString(CultureInfo.InvariantCulture)),
                new (UserContextManager.ROLE_CLAIM_TYPE, GetRole(tokenType))
            };

            var token = new JwtSecurityToken(
                AuthOptions.ISSUER,
                AuthOptions.AUDIENCE,
                notBefore: now,
                claims: claims,
                expires: expiresIn,
                signingCredentials: new SigningCredentials(AuthOptions.GetSecretKey(tokenType, configuration), SecurityAlgorithms.HmacSha256)
            );
            
            return (new JwtSecurityTokenHandler().WriteToken(token), expiresIn);
        }

        public static IEnumerable<Claim> EvaluateToken(this string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            IEnumerable<Claim> claims = new List<Claim>();
            if (tokenHandler.CanReadToken(token))
            {
                try
                {
                    var jwtToken = tokenHandler.ReadJwtToken(token);
                    claims = jwtToken.Claims;
                }
                catch { }
            }
            return claims;
        }

        public static bool IsTokenValid(this IEnumerable<Claim> claims)
        {
            string id = claims.GetUserId();
            string expirationDateTime = claims.GetExpirationDate();

            return !string.IsNullOrEmpty(expirationDateTime)
                   && !string.IsNullOrEmpty(id)
                   && DateTime.Parse(expirationDateTime, CultureInfo.InvariantCulture) > DateTime.UtcNow;
        }

        public static string GetUserId(this IEnumerable<Claim> claims)
        {
            return claims.GetClaim(UserContextManager.ID_CLAIM_TYPE);
        }
        
        public static string GetExpirationDate(this IEnumerable<Claim> claims)
        {
            return claims.GetClaim(UserContextManager.EXP_DATE_CLAIM_TYPE);
        }

        public static string GetRole(this IEnumerable<Claim> claims)
        {
            return claims.GetClaim(UserContextManager.ROLE_CLAIM_TYPE);
        }
        
        private static string GetClaim(this IEnumerable<Claim> claims, string type)
        {
            return claims
                .FirstOrDefault(claim => claim.Type == type)?
                .Value ?? string.Empty;
        }


        private static string GetRole(AuthOptions.TokenType tokenType)
        {
            return 
                tokenType == AuthOptions.TokenType.AppAccess || tokenType == AuthOptions.TokenType.AppRefresh 
                    ? AuthOptions.APP_ROLE
                    : AuthOptions.USER_ROLE;
        }
    }
}
