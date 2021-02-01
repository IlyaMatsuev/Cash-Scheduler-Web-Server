using System;
using System.Collections.Generic;

namespace CashSchedulerWebServer.Exceptions
{
    public class CashSchedulerException : Exception
    {
        public string Code { get; } = "400";
        public Dictionary<string, object> Fields { get; } = new Dictionary<string, object>();

        public CashSchedulerException(string message) : base(message)
        {
        }

        public CashSchedulerException(string message, string code) : base(message)
        {
            Code = code;
        }

        public CashSchedulerException(string message, string[] fields) : base(message)
        {
            Fields = new Dictionary<string, object> {{"fields", fields}};
        }
    }
}
