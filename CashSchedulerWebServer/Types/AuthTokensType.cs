using GraphQL.Types;

namespace CashSchedulerWebServer.Types
{
    public class AuthTokensType : ObjectGraphType<AuthTokensType>
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }

        public AuthTokensType()
        {
            Field(x => x.AccessToken, nullable: false);
            Field(x => x.RefreshToken, nullable: false);
        }

        public AuthTokensType(string accessToken, string refreshToken)
        {
            AccessToken = accessToken;
            RefreshToken = refreshToken;
        }
    }
}
