using System.Text.Encodings.Web;
using System.Threading.Tasks;
using CashSchedulerWebServer.Auth.Contracts;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CashSchedulerWebServer.Auth.AuthenticationHandlers
{
    public class CashSchedulerAuthenticationHandler : AuthenticationHandler<CashSchedulerAuthenticationOptions>
    {
        private IUserContextManager UserContextManager { get; }
        
        public CashSchedulerAuthenticationHandler(
            IUserContextManager userContextManager,
            IOptionsMonitor<CashSchedulerAuthenticationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock
            ) : base(options, logger, encoder, clock)
        {
            UserContextManager = userContextManager;
        }

        
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var userPrincipal = UserContextManager.GetUserPrincipal();
            var ticket = new AuthenticationTicket(userPrincipal, Scheme.Name);
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
