using CashSchedulerWebServer.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace CashSchedulerWebServer.Authentication
{
    public static class JwtTokenExtensions
    {
        public static (string token, DateTime expiresIn) GenerateToken(this User user, AuthOptions.TokenType tokenType, IConfiguration configuration)
        {
            var now = DateTime.UtcNow;
            var expiresIn = now.AddMinutes(AuthOptions.GetTokenLifetime(tokenType, configuration));
            IEnumerable<Claim> claims = new List<Claim>
            {
                new Claim("ExpirationDateTime", expiresIn.ToString()),
                new Claim("Id", user.Id.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: AuthOptions.ISSUER,
                audience: AuthOptions.AUDIENCE,
                notBefore: now,
                claims: claims,
                expires: expiresIn,
                signingCredentials: new SigningCredentials(AuthOptions.GetSecretKey(tokenType, configuration), SecurityAlgorithms.HmacSha256)
            );
            
            return (new JwtSecurityTokenHandler().WriteToken(token), expiresIn);
        }

        public static IEnumerable<Claim> EvaluateToken(this string token)
        {
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
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
    }
}
