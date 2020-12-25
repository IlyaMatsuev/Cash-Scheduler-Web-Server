using CashSchedulerWebServer.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Text;

namespace CashSchedulerWebServer.Authentication
{
    public class AuthOptions
    {
        public const string TYPE_TOKEN_SEPARATOR = " ";
        public const string AUTHENTICATION_TYPE = "Bearer";
        public const string ISSUER = "CashSchedulerServer";
        public const string AUDIENCE = "CashSchedulerClient";

        public const string EMAIL_REGEX = @"^[\w!#$%&'*+\-/=?\^_`{|}~]+(\.[\w!#$%&'*+\-/=?\^_`{|}~]+)*@((([\-\w]+\.)+[a-zA-Z]{2,4})|(([0-9]{1,3}\.){3}[0-9]{1,3}))\z";
        public const string PASSWORD_REGEX = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,15}$";

        public static SymmetricSecurityKey GetSecretKey(TokenType tokenType, IConfiguration configuration)
        {
            string secret = tokenType switch
            {
                TokenType.ACCESS => configuration["App:Auth:AccessTokenSecret"],
                TokenType.REFRESH => configuration["App:Auth:RefreshTokenSecret"],
                _ => throw new CashSchedulerException("There is no such token type"),
            };
            return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret));
        }

        public static int GetTokenLifetime(TokenType tokenType, IConfiguration configuration)
        {
            string lifetime = tokenType switch
            {
                TokenType.ACCESS => configuration["App:Auth:AccessTokenLifetime"],
                TokenType.REFRESH => configuration["App:Auth:RefreshTokenLifetime"],
                _ => throw new CashSchedulerException("There is no such token type"),
            };
            return Convert.ToInt32(lifetime);
        }


        public enum TokenType
        {
            ACCESS,
            REFRESH
        }
    }
}
