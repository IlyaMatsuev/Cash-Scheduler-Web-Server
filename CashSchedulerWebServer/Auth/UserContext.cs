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
            return Convert.ToInt32(HttpContext.User?.Claims.GetUserId() ?? "-1");
        }
    }
}
