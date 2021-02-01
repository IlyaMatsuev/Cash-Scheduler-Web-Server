using System.Security.Claims;

namespace CashSchedulerWebServer.Auth.Contracts
{
    public interface IUserContext
    {
        int GetUserId();
        ClaimsPrincipal GetUserPrincipal();
    }
}
