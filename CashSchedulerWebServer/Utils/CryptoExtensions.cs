using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CashSchedulerWebServer.Utils
{
    public static class CryptoExtensions
    {
        public static string Hash(this string input)
        {
            using SHA256 sha = SHA256.Create();
            return Encoding.ASCII.GetString(sha.ComputeHash(Encoding.ASCII.GetBytes(input)));
        }
    }
}
