using GraphQL;
using System.Collections.Generic;

namespace CashSchedulerWebServer.Exceptions
{
    public class CashSchedulerException : ExecutionError
    {
        public CashSchedulerException(string message) : base(message)
        {
        }

        public CashSchedulerException(string message, string code) : base(message)
        {
            Code = code;
        }

        public CashSchedulerException(string message, string[] fields)
            : base(message, new Dictionary<string, object> { { "fields", fields } })
        {
        }
    }
}
