using GraphQL.Types;

namespace CashSchedulerWebServer.Types
{
    public class AuthTokensType : ObjectGraphType<AuthTokensType>
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }

        public AuthTokensType()
        {
            Field("access_token", x => x.AccessToken, nullable: false);
            Field("refresh_token", x => x.RefreshToken, nullable: false);
        }

        public AuthTokensType(string accessToken, string refreshToken)
        {
            AccessToken = accessToken;
            RefreshToken = refreshToken;
        }
    }
}
