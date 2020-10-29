using CashSchedulerWebServer.Models;
using CashSchedulerWebServer.Types;
using System.Threading.Tasks;

namespace CashSchedulerWebServer.Authentication.Contracts
{
    public interface IAuthenticator
    {
        Task<AuthTokensType> Login(string email, string password);
        Task<User> Logout();
        Task<User> Register(User newUser);
        Task<AuthTokensType> Token(string email, string refreshToken);
        bool HasAccess(string accessToken);
        Task<string> CheckEmail(string email);
        Task<string> CheckCode(string email, string code);
        Task<User> ResetPassword(string email, string code, string password);
    }
}
