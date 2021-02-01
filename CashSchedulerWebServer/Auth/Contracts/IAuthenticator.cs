using System.Threading.Tasks;
using CashSchedulerWebServer.Models;
using CashSchedulerWebServer.Mutations.Users;

namespace CashSchedulerWebServer.Auth.Contracts
{
    public interface IAuthenticator
    {
        Task<AuthTokens> Login(string email, string password);
        Task<User> Logout();
        Task<User> Register(User newUser);
        Task<AuthTokens> Token(string email, string refreshToken);
        Task<string> CheckEmail(string email);
        Task<string> CheckCode(string email, string code);
        Task<User> ResetPassword(string email, string code, string password);
    }
}
