using System.Threading.Tasks;
using CashSchedulerWebServer.Models;
using CashSchedulerWebServer.Mutations.Users;

namespace CashSchedulerWebServer.Auth.Contracts
{
    public interface IAuthService
    {
        Task<AuthTokens> Login(string email, string password);

        Task<AuthTokens> AppLogin(string appToken);

        Task<User> Logout();

        Task<User> LogoutConnectedApps();

        Task<User> Register(User newUser);

        Task<AuthTokens> Token(string email, string refreshToken);

        Task<string> GenerateAppToken();

        Task<string> CheckEmail(string email);

        Task<string> CheckCode(string email, string code);

        Task<User> ResetPassword(string email, string code, string password);
    }
}
