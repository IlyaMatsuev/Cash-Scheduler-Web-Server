using System;
using System.Security.Cryptography;
using System.Text;

namespace CashSchedulerWebServer.Utils
{
    public static class CryptoExtensions
    {
        public static string Hash(this string input)
        {
            // TODO: add salt
            using SHA256 sha = SHA256.Create();
            return Encoding.ASCII.GetString(sha.ComputeHash(Encoding.ASCII.GetBytes(input)));
        }

        public static string Code(this string input)
        {
            // TODO: implement a better code generation
            string code = string.Empty;
            var random = new Random();
            for (int i = 0; i < 7; i++)
            {
                code += random.Next(0, 9);
            }
            return code;
        }
    }
}
