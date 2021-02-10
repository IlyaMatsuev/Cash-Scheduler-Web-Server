using System;
using System.Linq;
using CashSchedulerWebServer.Auth.Contracts;
using Microsoft.AspNetCore.Http;

namespace CashSchedulerWebServer.Auth
{
    public class UserContext : IUserContext
    {
        private HttpContext HttpContext { get; }
        
        public UserContext(IHttpContextAccessor httpContextAccessor)
        {
            HttpContext = httpContextAccessor.HttpContext;
        }
        
        
        public int GetUserId()
        {
            string userId = HttpContext.User?.Claims
                .FirstOrDefault(claim => claim.Type == UserContextManager.ID_CLAIM_TYPE)?.Value ?? "-1";
            
            return Convert.ToInt32(userId);
        }
    }
}
