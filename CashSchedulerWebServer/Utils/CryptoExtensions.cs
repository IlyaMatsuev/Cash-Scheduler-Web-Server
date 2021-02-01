using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace CashSchedulerWebServer.Utils
{
    public static class CryptoExtensions
    {
        public static string Hash(this string input, IConfiguration configuration)
        {
            using var sha = SHA256.Create();
            return Encoding.ASCII.GetString(sha.ComputeHash(Encoding.ASCII.GetBytes(input + configuration["App:Auth:PasswordSalt"])));
        }

        public static string Code(this string input, IConfiguration configuration)
        {
            var random = new Random();
            return string.Concat(
                Enumerable.Range(0, Convert.ToInt32(configuration["App:Auth:EmailVerificationCodeLength"]))
                    .Select(i => random.Next(0, 9).ToString())
            );
        }
    }
}
