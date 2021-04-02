using System;
using System.Text;
using CashSchedulerWebServer.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace CashSchedulerWebServer.Auth
{
    public static class AuthOptions
    {
        public const string AUTH_POLICY = "UserPolicy";
        public const string USER_ROLE = "User";
        public const string APP_ROLE = "App";
        public const string SALESFORCE_ROLE = "Salesforce";

        public const string TYPE_TOKEN_SEPARATOR = " ";
        public const string ISSUER = "CashSchedulerServer";
        public const string AUDIENCE = "CashSchedulerClient";

        public const string EMAIL_REGEX = @"^[\w!#$%&'*+\-/=?\^_`{|}~]+(\.[\w!#$%&'*+\-/=?\^_`{|}~]+)*@((([\-\w]+\.)+[a-zA-Z]{2,4})|(([0-9]{1,3}\.){3}[0-9]{1,3}))\z";
        public const string PHONE_REGEX = @"^(\+{0,})(\d{0,})([(]{1}\d{1,3}[)]{0,}){0,}(\s?\d+|\+\d{2,3}\s{1}\d+|\d+){1}[\s|-]?\d+([\s|-]?\d+){1,2}(\s){0,}$";
        public const string PASSWORD_REGEX = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,15}$";

        public static SymmetricSecurityKey GetSecretKey(TokenType tokenType, IConfiguration configuration)
        {
            string secret = tokenType switch
            {
                TokenType.Access => configuration["App:Auth:AccessTokenSecret"],
                TokenType.AppAccess => configuration["App:Auth:AppAccessTokenSecret"],
                TokenType.Refresh => configuration["App:Auth:RefreshTokenSecret"],
                TokenType.AppRefresh => configuration["App:Auth:AppRefreshTokenSecret"],
                _ => throw new CashSchedulerException("There is no such token type"),
            };
            return new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret));
        }

        public static int GetTokenLifetime(TokenType tokenType, IConfiguration configuration)
        {
            string lifetime = tokenType switch
            {
                TokenType.Access => configuration["App:Auth:AccessTokenLifetime"],
                TokenType.AppAccess => configuration["App:Auth:AccessTokenLifetime"],
                TokenType.Refresh => configuration["App:Auth:RefreshTokenLifetime"],
                TokenType.AppRefresh => configuration["App:Auth:RefreshTokenLifetime"],
                _ => throw new CashSchedulerException("There is no such token type"),
            };
            return Convert.ToInt32(lifetime);
        }


        public enum TokenType
        {
            Access,
            Refresh,
            AppAccess,
            AppRefresh
        }
    }
}
