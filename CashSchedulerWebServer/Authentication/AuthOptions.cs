using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace CashSchedulerWebServer.Authentication
{
    public class AuthOptions
    {
        public const string AUTHENTICATION_TYPE = "Bearer";
        public const string ISSUER = "CashSchedulerServer";
        public const string AUDIENCE = "CashSchedulerClient";

        // TODO: move these parameters to secret file
        public const int ACCESS_TOKEN_LIFETIME = 60;
        public const int REFRESH_TOKEN_LIFETIME = 10080;
        public const int EMAIL_VERIFICATION_CODE_LIFETIME = 5;

        private const string ACCESS_TOKEN_SECRET = "sexdfcghjbkmllmknbhvgcftxdrzswsrxdcfvghbjklokpjihuigyftrxcfvg";
        private const string REFRESH_TOKEN_SECRET = "iwbjqwdqhbwdbqhwdbqwiqojfowejiuwejfoiwqjdlqndkanslfjpjoiqruih3ru";

        public const string EMAIL_REGEX = @"^[\w!#$%&'*+\-/=?\^_`{|}~]+(\.[\w!#$%&'*+\-/=?\^_`{|}~]+)*@((([\-\w]+\.)+[a-zA-Z]{2,4})|(([0-9]{1,3}\.){3}[0-9]{1,3}))\z";
        public const string PASSWORD_REGEX = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\da-zA-Z]).{8,15}$";

        public static SymmetricSecurityKey GetSecretKey(TokenType tokenType)
        {
            SymmetricSecurityKey symmetricKey = null;
            switch (tokenType)
            {
                case TokenType.ACCESS:
                    symmetricKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(ACCESS_TOKEN_SECRET));
                    break;
                case TokenType.REFRESH:
                    symmetricKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(REFRESH_TOKEN_SECRET));
                    break;
            }
            return symmetricKey;
        }

        public static int GetTokenLifetime(TokenType tokenType)
        {
            int lifetime = 0;
            switch(tokenType)
            {
                case TokenType.ACCESS:
                    lifetime = ACCESS_TOKEN_LIFETIME;
                    break;
                case TokenType.REFRESH:
                    lifetime = REFRESH_TOKEN_LIFETIME;
                    break;
            }
            return lifetime;
        }


        public enum TokenType
        {
            ACCESS,
            REFRESH
        }
    }
}
