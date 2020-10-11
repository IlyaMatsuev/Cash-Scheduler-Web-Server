using System;

namespace CashSchedulerWebServer.Exceptions
{
    public class CashSchedulerException : Exception
    {
        public CashSchedulerException(string message) : base(message)
        {

        }
    }
}
