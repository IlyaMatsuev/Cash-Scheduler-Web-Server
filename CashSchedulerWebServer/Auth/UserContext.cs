using System;
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
            string userIdFromClaims = HttpContext?.User?.Claims.GetUserId();
            string userId = string.IsNullOrEmpty(userIdFromClaims)
                ? "-1"
                : userIdFromClaims;
            
            return Convert.ToInt32(userId);
        }
    }
}
