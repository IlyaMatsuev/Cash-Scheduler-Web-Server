using System.Security.Claims;

namespace CashSchedulerWebServer.Auth.Contracts
{
    public interface IUserContextManager
    {
        ClaimsPrincipal GetUserPrincipal();
    }
}
